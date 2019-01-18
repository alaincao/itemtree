
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
		public async Task<IActionResult> List(bool includeImages=true, bool includeInactives=true)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"TestimonialList: START" );

			var rv = await (new Services.TestimonialController( DataContext, PageHelper )).List( includeImages:includeImages, includeInactives:includeInactives );
			var model = new ListModel{
								UrlImgNotFound	= PageHelper.ResolveRoute( ItemTTT.wwwroot.ImgNotFound ),
								Items			= rv.Result ?? (new DTOs.Testimonial[]{}),
							};

			logHelper.AddLogMessage( $"TestimonialList: END" );
			return View( model );
		}
		public struct ListModel
		{
			public string				UrlImgNotFound;
			public DTOs.Testimonial[]	Items;
		}
	}
}
