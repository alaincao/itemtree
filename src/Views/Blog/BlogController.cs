
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class BlogController : BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public BlogController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.BlogEdit )]
		public IActionResult Edit()
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogEdit: START" );

			logHelper.AddLogMessage( $"BlogEdit: END" );
			return View();
		}
	}
}
