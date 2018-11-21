using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class HomeController : Controller
	{
		private readonly PageHelper		PageHelper;
		private readonly Models.ItemTTT	DataContext;

		public HomeController(Models.ItemTTT dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.Home )]
		[HttpHead( Routes.Home )]
		public IActionResult Index()
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( "Index START" );

			PageHelper.Parameters.PageTitle = "Hello world";

			logHelper.AddLogMessage( "Index END" );
			PageHelper.Parameters["Logs"] = logHelper.GetLogLines();
			return View();
		}

		[HttpGet( Routes.FooBar )]
		public IActionResult FooBar(string lang)
		{
			PageHelper.Parameters.PageTitle = "Foo / bar";

			return View(new{ Lang=""+PageHelper.CurrentLanguage, Parameters=PageHelper.Parameters });
		}
	}
}
