using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		[HttpGet( Routes.Login )]
		public IActionResult Login()
		{
			if( PageHelper.IsAuthenticated )
				// Already authenticated
				return Redirect( Routes.AdminHome );
			return View();
		}

		[HttpPost( Routes.Login )]
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

		[HttpGet( Routes.Logout )]
		public async Task<IActionResult> Logout()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			var rv = await ( new Services.LoginController(DataContext, PageHelper){ ControllerContext=ControllerContext } )
												.Logout();
			return Redirect( Routes.Home1 );
		}
	}
}
