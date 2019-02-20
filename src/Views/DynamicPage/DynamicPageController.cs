
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class DynamicPageController : BaseController
	{
		private const string	ShowView	= Startup.ViewsLocation+"DynamicPage/Show.cshtml";

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public DynamicPageController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.DynamicPageStatic )]
		public Task<IActionResult> StaticDemo()
		{
			return Show( lang:PageHelper.CurrentLanguage, itemCode:"static-demo" );
		}

		[HttpGet( Routes.DynamicPageShow )]
		public async Task<IActionResult> Show(Languages lang, string itemCode)
		{
			var rv = await (new Services.DynamicPageController( DataContext, PageHelper )).Get( itemCode:itemCode, language:lang );
			if(! rv.Success )
				return ObjectNotFound();

			var model = new ShowModel {	ItemCode	= itemCode,
										Text		= rv.Result };
			return View( ShowView, model );
		}
		public struct ShowModel
		{
			public string	ItemCode;
			public string	Text;
		}
	}
}
