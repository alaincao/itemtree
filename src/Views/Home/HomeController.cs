using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class HomeController : Controller
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public HomeController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.Home1 )]
		[HttpGet( Routes.Home2 )]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet( Routes.Error )]
		[HttpGet( Routes.ErrorStatus )]
		public IActionResult Error(int? status=null)
		{
			ViewBag.Status = status;
			return View();
		}

		[HttpGet("/kaboom")]
		public IActionResult Kaboom()
		{
			// nb: Test exception behaviour (i.e. run in release mode ...)
			throw new Utils.TTTException( "Kaboom" );
		}
	}
}
