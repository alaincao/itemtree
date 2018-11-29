using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class LoginController : Controller
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		internal const string	LoginHardCoded	= "admin";
		// TODO: Server-side(?) session & seed creation (encrypt password before POSTing - as it is in original YourCar)
		private const string	CurrentSeed		= "";

		public LoginController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.LoginAPI )]
		[HttpPost( Routes.LoginAPI )]
		public async Task<Common.TTTServiceResult> Login(string login, string password)
		{
			var logHelper = PageHelper.ScopeLogs;
			string errorMessage;

			if( PageHelper.IsAutenticated )
			{
				logHelper.AddLogMessage( $"Login: Already logged-in" );
				return new Common.TTTServiceResult{ Success=true, ErrorMessage="Already logged-in" };
			}

			logHelper.AddLogMessage( $"Login: Check login empty" );
			if( string.IsNullOrWhiteSpace(login) )
			{
				errorMessage = "Please enter your login";
				goto FAILED;
			}
			logHelper.AddLogMessage( "Login: Check password empty" );
			if( string.IsNullOrWhiteSpace(password) )
			{
				errorMessage = "Please enter your password";
				goto FAILED;
			}

			logHelper.AddLogMessage( "Login: Check login" );
			if( login.ToLower() != Services.LoginController.LoginHardCoded )
			{
				errorMessage = "Login failed !";
				goto FAILED;
			}

			logHelper.AddLogMessage( "Login: Check password" );
			if(! await Services.LoginController.CheckLogin(DataContext, password) )
			{
				errorMessage = "Login failed !";
				goto FAILED;
			}

			logHelper.AddLogMessage( "Login: success" );
			var claimsIdentity = new ClaimsIdentity( new Claim[]
											{
												new Claim( ClaimTypes.Name, Services.LoginController.LoginHardCoded, ClaimValueTypes.String, issuer:"ItemTTT login" ),
											}, Startup.AuthScheme );
			var authProperties = new AuthenticationProperties{};
			await HttpContext.SignInAsync( Startup.AuthScheme, new ClaimsPrincipal(claimsIdentity), authProperties );

			return new Common.TTTServiceResult{ Success=true };

		FAILED:
			logHelper.AddLogMessage( "Login: failed" );
		
			var rv = new Common.TTTServiceResult{ Success=false, ErrorMessage=errorMessage };
			if( Utils.IsDebug )
				rv.Log = logHelper.GetLogLines();
			return rv;
		}

		[HttpGet( Routes.LogoutAPI )]
		[HttpPost( Routes.LogoutAPI )]
		public async Task<Common.TTTServiceResult> Logout()
		{
			if(! HttpContext.User.Identity.IsAuthenticated )
			{
				return new Common.TTTServiceResult{ Success=false, ErrorMessage="Error: not logged-in" };
			}
			else
			{
				await HttpContext.SignOutAsync( Startup.AuthScheme );
				return new Common.TTTServiceResult{ Success=true };
			}
		}

		[HttpGet( Routes.ChangePassword )]
		[HttpPost( Routes.ChangePassword )]
		public async Task<object> ChangePassword(string oldPassword, string newPassword)
		{
			if(! PageHelper.IsAutenticated )
				// Hacker?
				return NotFound();

			try
			{
				if( string.IsNullOrWhiteSpace(newPassword) )
					throw new Common.TTTException( "The new password has not been specified" );

				var originalPasswordOK = await CheckLogin( DataContext, oldPassword );
				if(! originalPasswordOK )
					throw new Common.TTTException( "The original password is invalid" );

				var newPasswordHash = Utils.GetMd5Sum( newPassword );
				var entry = await DataContext.Configurations.Where( v=>v.Key == Models.Configuration.Key_PasswordHash ).SingleAsync();
				entry.Value = newPasswordHash;
				await DataContext.SaveChangesAsync();

				return new{ success = true };
			}
			catch( Common.TTTException ex )
			{
				// Don't send the whole exception; Just the error message
				return new{	succes	= false,
							error	= ex.Message };
			}
		}

		private static async Task<bool> CheckLogin(Models.ItemTTTContext dc, string password)
		{
			string expected;
			{
				var hashedPassword = await dc.Configurations.Where( v=>v.Key == Models.Configuration.Key_PasswordHash ).Select( v=>v.Value ).SingleAsync();
				expected = hashedPassword + CurrentSeed;
				expected = Utils.GetMd5Sum( expected );
			}

			string challenge;
			{
				var hashedPassword = Utils.GetMd5Sum( password );
				challenge = hashedPassword + CurrentSeed;
				challenge = Utils.GetMd5Sum( challenge );
			}

			return (challenge == expected);
		}
	}
}
