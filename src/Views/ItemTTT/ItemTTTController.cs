using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class ItemTTTController : BaseController
	{
		private const string	ListPartial		= Startup.ViewsLocation+"ItemTTT/List_List.cshtml";
		private const string	GridPartial		= Startup.ViewsLocation+"ItemTTT/List_Grid.cshtml";

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		internal const SortingFields	SortingFieldDefault	= SortingFields.name;
		private const ViewModes			ViewModeDefault		= ViewModes.grid;

		public enum ViewModes		{ grid, list };
		public enum SortingFields	{ price, price_desc, name, name_desc }

		internal static string CreateUrlDetails(Languages language, string itemCode)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(itemCode), typeof(ItemTTTController), $"Missing parameter {nameof(itemCode)}" );
			var url = Routes.ItemDetails
								.Replace( Language.RouteParameter, ""+language )
								.Replace( "{itemCode}", itemCode );
			return url;
		}

		public ItemTTTController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.ItemsList )]
		public async Task<IActionResult> List(ViewModes? viewMode=null, SortingFields? sortingField=null, bool noLayout=false)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemsList: START: {nameof(viewMode)}:{viewMode},  {nameof(sortingField)}:{sortingField},  {nameof(noLayout)}:{noLayout}" );

			viewMode = Utils.GetSetCookie( HttpContext, specified:viewMode, fallback:ViewModeDefault );
			logHelper.AddLogMessage( $"ItemsList: Using {nameof(viewMode)}:{viewMode}" );
			sortingField = Utils.GetSetCookie( HttpContext, specified:sortingField, fallback:SortingFieldDefault );
			logHelper.AddLogMessage( $"ItemsList: Using {nameof(sortingField)}:{sortingField}" );

			var rv = await (new Services.ItemController(DataContext, PageHelper)).Query( itemCode:null, sortingField:sortingField.Value );
			if(! rv.Success )
				throw NewUnexpectedException( rv.ErrorMessage );

			string view;
			switch( viewMode.Value )
			{
				case ViewModes.list:	view = ListPartial;		break;
				case ViewModes.grid:	view = GridPartial;		break;
				default: throw new NotImplementedException( $"Invalid value '{viewMode}' for parameter '{nameof(viewMode)}'" );
			}
			var model = new ListModel{	View			= view,
										ViewMode		= viewMode.Value,
										SortingField	= sortingField.Value,
										Items			= rv.Result };

			if(! noLayout )
			{
				logHelper.AddLogMessage( $"ItemsList: END using default view" );
				return View( model );
			}
			else
			{
				logHelper.AddLogMessage( $"ItemsList: END using view '{view}'" );
				return View( view, model );
			}
		}
		public struct ListModel
		{
			public string			View;
			public ViewModes		ViewMode;
			public SortingFields	SortingField;
			public DTOs.Item[]		Items;
		}

		[HttpGet( Routes.ItemDetails )]
		public async Task<IActionResult> Details(string itemCode)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemDetails: START: {nameof(itemCode)}:{itemCode}" );

			var rv = await (new Services.ItemController( DataContext, PageHelper )).Query( itemCode:itemCode, sortingField:null );
			if(! rv.Success )
				throw NewUnexpectedException( rv.ErrorMessage );
			if( rv.Result.Length == 0 )
				return ObjectNotFound();
			Utils.Assert( rv.Result.Length == 1, this, $"Invalid result count:{rv.Result.Length} ; expected 1" );
			var item = rv.Result[0];

			var result = View( item );
			logHelper.AddLogMessage( $"ItemDetails: END" );
			return result;
		}

		[HttpGet( Routes.ItemEdit )]
		public async Task<IActionResult> Edit(string itemCode)
		{
			PageHelper.ScopeLogs.AddLogMessage( $"ItemEdit: START" );
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			return await Details( itemCode );
		}
	}
}
