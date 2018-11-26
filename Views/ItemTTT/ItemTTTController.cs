using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class ItemTTTController : Controller
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public ItemTTTController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.ItemDetails )]
		public IActionResult Details(string lang, string code)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemDetails: Lang: '{lang}' ; Code: '{code}'" );

			logHelper.AddLogMessage( $"ItemDetails: Retreive from database" );
			var item = DataContext.Items.Where( v=>v.Code == code ).SingleOrDefault();
			if( item == null )
				return NotFound();

			if(! item.Active )
				// TODO: Allow when authenticated
				return NotFound();

			return View( item );
		}
	}
}
