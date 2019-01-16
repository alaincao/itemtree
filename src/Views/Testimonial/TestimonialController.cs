
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views.Testimonial
{
	public class TestimonialController : BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public TestimonialController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.TestimonialList )]
		public async Task<IActionResult> List(bool includeImages=true)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"TestimonialList: START" );

			var rv = await (new Services.TestimonialController( DataContext, PageHelper )).List( includeImages:includeImages );
			var model = new ListModel{ Items = rv.Result ?? (new DTOs.Testimonial[]{}) };

			logHelper.AddLogMessage( $"TestimonialList: END" );
			return View( model );
		}
		public struct ListModel
		{
			public DTOs.Testimonial[]	Items;
		}
	}
}
