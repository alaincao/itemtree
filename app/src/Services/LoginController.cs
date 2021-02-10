using System;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Services
{
	public class LoginController : Views.BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		private const string	LoginDefaultName		= "admin";	// Changeme after installation on production !!!
		private const string	LoginDefaultPassword	= "123";

		private class UserNode
		{
			public CredentialNode	Credentials		{ get; set; }
		}
		private class CredentialNode
		{
			public string	Seed		{ get; set; }
			public string	Hash		{ get; set; }
		}

		public LoginController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		internal static async Task Initialize(LogHelper logHelper, IServiceProvider initializationServices)
		{
			logHelper.AddLogMessage( $"{nameof(LoginController)}: START" );

			logHelper.AddLogMessage( $"{nameof(LoginController)}: Check for existence of at least one login" );
			var cwd = Tree.Cwd.New( logHelper, services:initializationServices, rootSuffix:Models.TreeNode.RootUsersSuffix );
			var usersCount = ( await cwd.TreeHelper.GetChildNodes(cwd) ).Length;
			if( usersCount > 0 )
			{
				logHelper.AddLogMessage( $"{nameof(LoginController)}: There are already {usersCount} users present in the database" );
				goto EXIT;
			}

			logHelper.AddLogMessage( $"{nameof(LoginController)}: There are no users available ; Create the default one: '{LoginDefaultName}/{LoginDefaultPassword}'" );
			await SetPassword( logHelper, initializationServices, LoginDefaultName, LoginDefaultPassword, createIfNotExist:true );

		EXIT:
			logHelper.AddLogMessage( $"{nameof(LoginController)}: END" );
		}

		[HttpGet( Routes.LoginAPI )]
		[HttpPost( Routes.LoginAPI )]
		public async Task<Utils.TTTServiceResult> Login(string login, string password)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				string errorMessage;

				if( PageHelper.IsAuthenticated )
				{
					logHelper.AddLogMessage( $"{nameof(Login)}: Already logged-in" );
					return new Utils.TTTServiceResult( PageHelper, errorMessage:"Already logged-in" );
				}

				logHelper.AddLogMessage( $"{nameof(Login)}: Check login empty" );
				if( string.IsNullOrWhiteSpace(login) )
				{
					errorMessage = "Please enter your login";
					goto FAILED;
				}
				logHelper.AddLogMessage( $"{nameof(Login)}: Check password empty" );
				if( string.IsNullOrWhiteSpace(password) )
				{
					errorMessage = "Please enter your password";
					goto FAILED;
				}

				logHelper.AddLogMessage( $"{nameof(Login)}: Check login" );
				{
					var rc = await CheckLogin( logHelper, HttpContext.RequestServices, login, password );
					if(! rc )
					{
						errorMessage = "Login failed !";
						goto FAILED;
					}
				}

				logHelper.AddLogMessage( $"{nameof(Login)}: success" );
				var claimsIdentity = new ClaimsIdentity( new Claim[]
												{
													new Claim( ClaimTypes.Name, login, ClaimValueTypes.String, issuer:"ItemTTT login" ),
												}, Startup.AuthScheme );
				var authProperties = new AuthenticationProperties{};
				await HttpContext.SignInAsync( Startup.AuthScheme, new ClaimsPrincipal(claimsIdentity), authProperties );

				return new Utils.TTTServiceResult( PageHelper );

			FAILED:
				logHelper.AddLogMessage( $"{nameof(Login)}: failed" );
				return new Utils.TTTServiceResult( PageHelper, errorMessage:errorMessage );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.LogoutAPI )]
		[HttpPost( Routes.LogoutAPI )]
		public async Task<Utils.TTTServiceResult> Logout()
		{
			if(! PageHelper.IsAuthenticated )
			{
				return new Utils.TTTServiceResult( PageHelper, errorMessage:"Error: not logged-in" );
			}
			else
			{
				await HttpContext.SignOutAsync( Startup.AuthScheme );
				return new Utils.TTTServiceResult( PageHelper );
			}
		}

		[HttpGet( Routes.ChangePassword )]
		[HttpPost( Routes.ChangePassword )]
		public async Task<Utils.TTTServiceResult> ChangePassword(string oldPassword, string newPassword, string login=null)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;

				if(! PageHelper.IsAuthenticated )
					return new Utils.TTTServiceResult( PageHelper, errorMessage:"Error: not logged-in" );

				if( string.IsNullOrWhiteSpace(newPassword) )
					throw new Utils.TTTException( "The new password has not been specified" );

				logHelper.AddLogMessage( $"{nameof(ChangePassword)}: Check current user's ({PageHelper.UserName}) password" );
				var originalPasswordOK = await CheckLogin( logHelper, HttpContext.RequestServices, PageHelper.UserName, oldPassword );
				if(! originalPasswordOK )
					throw new Utils.TTTException( "The current password is invalid" );

				logHelper.AddLogMessage( $"{nameof(ChangePassword)}: Set password" );
				await SetPassword( logHelper, HttpContext.RequestServices, login??PageHelper.UserName, newPassword );

				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		private static async Task<bool> CheckLogin(LogHelper logHelper, IServiceProvider services, string login, string password)
		{
			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Sanitize" );
			{
				var orig = login;
				login = Tree.Cwd.SanitizeName( login );
				if( orig != login.ToLower() )
				{
					logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Invalid chars" );
					goto FAILED;
				}
			}

			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Check login exists" );
			var cwd = Tree.Cwd.New( logHelper, services:services, rootSuffix:Models.TreeNode.RootUsersSuffix );
			cwd.Cd( login );
			var nodeJson = await cwd.TreeHelper.GetNodeData( cwd );
			if( string.IsNullOrWhiteSpace(nodeJson) )
			{
				logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Not found" );
				goto FAILED;
			}
			var node = nodeJson.JSONDeserialize<UserNode>();

			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Check password" );
			var expected = node.Credentials.Hash;
			var challenge = Utils.GetMd5Sum( password + node.Credentials.Seed );
			if( challenge != expected )
			{
				logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Invalid password" );
				goto FAILED;
			}
			return true;

		FAILED:
			return false;
		}

		private static async Task SetPassword(LogHelper logHelper, IServiceProvider services, string login, string password, bool createIfNotExist=false)
		{
			logHelper.AddLogMessage( $"{nameof(SetPassword)}: Sanitize" );
			{
				var orig = login;
				login = Tree.Cwd.SanitizeName( login );
				if( orig != login.ToLower() )
					throw new Utils.TTTException( "The login contains invalid chars" );
			}

			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Check already exists" );
			var cwd = Tree.Cwd.New( logHelper, services:services, rootSuffix:Models.TreeNode.RootUsersSuffix );
			cwd.Cd( login );
			UserNode node;
			{
				var nodeJson = await cwd.TreeHelper.GetNodeData( cwd );
				if( string.IsNullOrWhiteSpace(nodeJson) )
				{
					if(! createIfNotExist )
						throw new Utils.TTTException( "The specified login does not exist" );
					logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Does not exist => create new node" );

					await cwd.TreeHelper.CreateNode( cwd, meta:null, data:null );
					node = new UserNode{ Credentials=new CredentialNode{} };
				}
				else
				{
					logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Exists => parse JSON node" );
					node = nodeJson.JSONDeserialize<UserNode>();
				}
			}

			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Create seed & hash" );
			node.Credentials.Seed = Utils.GetRandomString( 8 );
			node.Credentials.Hash = Utils.GetMd5Sum( password + node.Credentials.Seed );

			logHelper.AddLogMessage( $"{nameof(CheckLogin)}: Save node" );
			{
				var nodeJson = node.JSONStringify();
				await cwd.TreeHelper.SetNodeData( cwd, nodeJson );
			}
		}
	}
}
