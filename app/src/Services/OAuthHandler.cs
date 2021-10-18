using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Services
{
	using OAuthScheme = AppSettingsKeys.OAuthSchemes;

	public class OAuthHandler : OAuthHandler<OAuthOptions>
	{
		public OAuthHandler(Microsoft.Extensions.Options.IOptionsMonitor<OAuthOptions> options, Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
			: base( options, logger, encoder, clock )
		{
		}

		internal static IDictionary<string,OAuthScheme> GetSchemes(LogHelper log, Microsoft.Extensions.Configuration.IConfigurationRoot configuration)
		{
			log.AddLogMessage( $"{nameof(OAuthHandler)}: GetSchemes()" );
			var dict = new Dictionary<string,OAuthScheme>();
			foreach( var item in configuration.GetSection( nameof(AppSettingsKeys.OAuthSchemes) ).GetChildren() )
			{
				log.AddLogMessage( $"{nameof(OAuthHandler)}: At scheme '{item.Key}'" );
				var scheme = new OAuthScheme{
						AllowAlLUsers			= bool.Parse( item[nameof(OAuthScheme.AllowAlLUsers)] ),
						AuthorizationEndpoint	= item[ nameof(OAuthScheme.AuthorizationEndpoint) ],
						TokenEndpoint			= item[ nameof(OAuthScheme.TokenEndpoint) ],
						ClientId				= item[ nameof(OAuthScheme.ClientId) ],
						ClientSecret			= item[ nameof(OAuthScheme.ClientSecret) ],
						SigningCallbackPath		= item[ nameof(OAuthScheme.SigningCallbackPath) ],
						UserInformationEndpoint	= item[ nameof(OAuthScheme.UserInformationEndpoint) ],
						UserClaimField			= item[ nameof(OAuthScheme.UserClaimField) ],
					};
				var scopes = item.GetSection( nameof(OAuthScheme.Scope) )?.GetChildren();
				if( scopes != null )
					scheme.Scope = scopes.Select( v=>v.Value ).ToArray();

				dict.Add( item.Key, scheme );
			}
			log.AddLogMessage( $"{nameof(OAuthHandler)}: Found '{dict.Count}' scheme(s)" );
			return dict;
		}

		protected override async Task<AuthenticationTicket> CreateTicketAsync(System.Security.Claims.ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
		{
			var services = Context.RequestServices;
			var log = services.GetRequiredService<LogHelper>();
			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): START '{Scheme.Name}'" );

			var startup = services.GetRequiredService<Startup>();
			var scheme = startup.OAuthSchemes[ Scheme.Name ];

			System.Text.Json.JsonElement claims;
			if( string.IsNullOrEmpty(Options.UserInformationEndpoint) )
			{
				// No need to use a back channel request to get user's claims => Directly use the tokens
				claims = tokens.Response.RootElement;
			}
			else
			{
				// Make a back channem request to get user's info  ; cf. example at https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers/blob/2f2a595270cd7b99085591cc7b12aabbdc61e72e/src/AspNet.Security.OAuth.Nextcloud/NextcloudAuthenticationHandler.cs#L42
				log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Request user info from '{Options.UserInformationEndpoint}'" );

				using var request = new System.Net.Http.HttpRequestMessage( System.Net.Http.HttpMethod.Get, Options.UserInformationEndpoint );
				request.Headers.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json") );
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Bearer", tokens.AccessToken );

				using var response = await Backchannel.SendAsync( request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted );
				if( ! response.IsSuccessStatusCode )
				{
					log.AddLogLines( $"An error occurred while retrieving the user profile: the remote server returned a '{response.StatusCode}' response with the following payload:\n"
								+ $"{response.Headers}\n{await response.Content.ReadAsStringAsync(Context.RequestAborted)}." );
					throw new System.Net.Http.HttpRequestException( "An error occurred while retrieving the user profile." );
				}

				log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Parse user info" );
				var payload = System.Text.Json.JsonDocument.Parse( await response.Content.ReadAsStringAsync(Context.RequestAborted) );
				claims = payload.RootElement;
			}

			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Get user tokens using field '{scheme.UserClaimField}'" );
			var claimedUserOrig = claims.GetString( scheme.UserClaimField );
			if( string.IsNullOrWhiteSpace(claimedUserOrig) )
			{
				var strTokens = System.Text.Json.JsonSerializer.Serialize( claims, new System.Text.Json.JsonSerializerOptions{ WriteIndented=true } );
				log.AddLogLines( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Tokens:\n{strTokens}" );
				throw new ApplicationException( $"Unable to find user from token '{scheme.UserClaimField}'" );
			}
			var claimedUserSanitized = Services.LoginController.SanitizeLogin( claimedUserOrig );
			var fallbackLogin = Services.LoginController.SanitizeLogin( $"{Scheme.Name}_{claimedUserOrig}" );

			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Search for user '{claimedUserOrig}'" );
			var cwd = Tree.Cwd.New( log, services:services, rootSuffix:Models.TreeNode.RootUsersSuffix );
			var logins = ( await cwd.TreeHelper.GetChildNodes(cwd) )
										.Select( v=>v.B )
										.OrderBy( v=> (v == fallbackLogin) ? 1  // Search first for a login looking like '<scheme>_<user>'
													: (v == claimedUserSanitized) ? 2  // Then for a login with the exact same login (eg. manually created ?)
													: 3 );  // Then all others

			string userLogin;
			Services.LoginController.UserNode userNode = null;
			foreach( var login in logins )
			using( cwd.PushDisposable(login) )
			{
				log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Check user '{cwd.Pwd()}'" );
				var nodeJson = await cwd.TreeHelper.GetNodeData( cwd );
				userNode = nodeJson.JSONDeserialize<Services.LoginController.UserNode>();

				if( userNode.Credentials?.OAuth?.TryGet(Scheme.Name)?.UserClaim != claimedUserOrig )
					continue;

				userLogin = login;
				goto FOUND;
			}

			if( scheme.AllowAlLUsers == false )
				throw new ApplicationException( $"User '{claimedUserOrig}' from OAuth scheme '{Scheme.Name}' is not authorized" );

			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): No match found => Create new node for login '{fallbackLogin}'" );
			using( cwd.PushDisposable(fallbackLogin) )
			{
				userLogin = fallbackLogin;

				userNode = new Services.LoginController.UserNode{
									Credentials = new Services.LoginController.CredentialNode{
											OAuth = new Dictionary<string,Services.LoginController.OAuthNode>{{ Scheme.Name, new Services.LoginController.OAuthNode{
													UserClaim = claimedUserOrig,
												} }},
										} };
				await cwd.TreeHelper.CreateNode( cwd, meta:null, data:userNode.JSONStringify() );
			}

		FOUND:
			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): Using login '{userLogin}'" );
			var principal = new ClaimsPrincipal( identity );
			var context = new OAuthCreatingTicketContext( principal, properties, Context, Scheme, Options, Backchannel, tokens, user:/*dummy*/new System.Text.Json.JsonElement() );
			context.RunClaimActions();  // nb: This should do nothing ...
			identity.AddClaim( new Claim( type:ClaimTypes.Name, value:userLogin, valueType:ClaimValueTypes.String, issuer:Scheme.Name) );
			await Events.CreatingTicket( context );
			var rc = new AuthenticationTicket( context.Principal!, context.Properties, Scheme.Name );
			log.AddLogMessage( $"{nameof(OAuthHandler)}: CreateTicketAsync(): END" );
			return rc;
		}
	}
}
