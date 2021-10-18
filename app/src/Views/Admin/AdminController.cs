using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Views
{
	public class AdminController : BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public AdminController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.AdminHome )]
		public async Task<IActionResult> Home()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			var log = HttpContext.RequestServices.GetRequiredService<LogHelper>();
			log.AddLogMessage( $"Check if user has password authentication enabled" );
			var cwd = Tree.Cwd.New( log, services:HttpContext.RequestServices, rootSuffix:Models.TreeNode.RootUsersSuffix );
			using( cwd.PushDisposable(PageHelper.UserName) )
			{
				var node = ( await cwd.TreeHelper.GetNodeData(cwd) ).JSONDeserialize<Services.LoginController.UserNode>();
				ViewBag.CurrentUserHasPassword = ( node.Credentials?.Hash != null );
			}

			return View();
		}

		[HttpGet( Routes.Login )]
		public IActionResult Login()
		{
			if( PageHelper.IsAuthenticated )
				// Already authenticated
				return Redirect( Routes.AdminHome );
			return View();
		}

		[HttpPost( Routes.LoginPassword )]
		public async Task<IActionResult> Login(string login, string password)
		{
			var rv = await ( new Services.LoginController(DataContext, PageHelper){ ControllerContext=ControllerContext } )
												.Login( login, password );
			if( rv.Success == true )
			{
				return Redirect( Routes.AdminHome );
			}
			else
			{
				ViewBag.PostedLogin		= login;
				ViewBag.PostedPassword	= password;
				ViewBag.ErrorMessage	= rv.ErrorMessage;
				return View();
			}
		}

		[HttpGet(Routes.LoginOAuth)]
		public IActionResult LoginOAuth(string scheme)
		{
			return Challenge( new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri=Routes.AdminHome }, scheme );
		}

		[HttpGet( Routes.Logout )]
		public async Task<IActionResult> Logout()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			var rv = await ( new Services.LoginController(DataContext, PageHelper){ ControllerContext=ControllerContext } )
												.Logout();
			return Redirect( Routes.Home1 );
		}

		[HttpGet( Routes.Redirections )]
		public IActionResult Redirections()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();
			return View();
		}

		[HttpGet( Routes.TreeBrowse )]
		public IActionResult TreeBrowse()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();
			return View();
		}
	}
}
