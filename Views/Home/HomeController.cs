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

		[HttpGet( Routes.Home )]
		public IActionResult Index()
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( "Index START" );

			PageHelper.Parameters.PageTitle = "Hello world";

			logHelper.AddLogMessage( "Index END" );
			PageHelper.Parameters["Logs"] = logHelper.GetLogLines();
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
			throw new System.ApplicationException( "Kaboom" );
		}
	}
}
