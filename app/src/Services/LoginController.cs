using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class LoginController : Views.BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		internal const string	LoginHardCoded	= "admin";
		private const string	PasswordSeed	= "ItemTTT";

		public LoginController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.LoginAPI )]
		[HttpPost( Routes.LoginAPI )]
		public async Task<Utils.TTTServiceResult> Login(string login, string password)
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
			if( login.ToLower() != Services.LoginController.LoginHardCoded )
			{
				errorMessage = "Login failed !";
				goto FAILED;
			}

			logHelper.AddLogMessage( $"{nameof(Login)}: Check password" );
			if(! await Services.LoginController.CheckLogin(DataContext, password) )
			{
				errorMessage = "Login failed !";
				goto FAILED;
			}

			logHelper.AddLogMessage( $"{nameof(Login)}: success" );
			var claimsIdentity = new ClaimsIdentity( new Claim[]
											{
												new Claim( ClaimTypes.Name, Services.LoginController.LoginHardCoded, ClaimValueTypes.String, issuer:"ItemTTT login" ),
											}, Startup.AuthScheme );
			var authProperties = new AuthenticationProperties{};
			await HttpContext.SignInAsync( Startup.AuthScheme, new ClaimsPrincipal(claimsIdentity), authProperties );

			return new Utils.TTTServiceResult( PageHelper );

		FAILED:
			logHelper.AddLogMessage( $"{nameof(Login)}: failed" );
			return new Utils.TTTServiceResult( PageHelper, errorMessage:errorMessage );
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
		public async Task<Utils.TTTServiceResult> ChangePassword(string oldPassword, string newPassword)
		{
			if(! PageHelper.IsAuthenticated )
				return new Utils.TTTServiceResult( PageHelper, errorMessage:"Error: not logged-in" );

			try
			{
				if( string.IsNullOrWhiteSpace(newPassword) )
					throw new Utils.TTTException( "The new password has not been specified" );

				var originalPasswordOK = await CheckLogin( DataContext, oldPassword );
				if(! originalPasswordOK )
					throw new Utils.TTTException( "The original password is invalid" );

				var newPasswordHash = Utils.GetMd5Sum( newPassword );
				var entry = await DataContext.Configurations.Where( v=>v.Key == Models.Configuration.Key_PasswordHash ).SingleAsync();
				entry.Value = newPasswordHash;
				await DataContext.SaveChangesAsync();

				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( Utils.TTTException ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		private static async Task<bool> CheckLogin(Models.ItemTTTContext dc, string password)
		{
			var expected = await dc.Configurations.Where( v=>v.Key == Models.Configuration.Key_PasswordHash ).Select( v=>v.Value ).SingleAsync();
			var challenge = Utils.GetMd5Sum( password + PasswordSeed );
			return (challenge == expected);
		}
	}
}
