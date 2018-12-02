using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class ItemTTTController : Controller
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		internal const OrderBys		OrderByDefault		= OrderBys.name;
		private const ViewTypes		ViewTypeDefault		= ViewTypes.grid;

		public enum ViewTypes	{ grid, list };
		public enum OrderBys	{ price, name }

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
		public async Task<IActionResult> List(ViewTypes? viewType=null, OrderBys? orderBy=null, bool noLayout=false)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemsList: START: {nameof(viewType)}:{viewType},  {nameof(orderBy)}:{orderBy},  {nameof(noLayout)}:{noLayout}" );

			viewType = Utils.GetSetCookie( HttpContext, specified:viewType, fallback:ViewTypeDefault );
			logHelper.AddLogMessage( $"ItemsList: Using {nameof(viewType)}:{viewType}" );
			orderBy = Utils.GetSetCookie( HttpContext, specified:orderBy, fallback:OrderByDefault );
			logHelper.AddLogMessage( $"ItemsList: Using {nameof(orderBy)}:{orderBy}" );

			var rv = await (new Services.ItemController(DataContext, PageHelper)).Query( itemCode:null, orderBy:orderBy.Value );
			if(! rv.Success )
				throw new Utils.TTTException( rv.ErrorMessage );

			var result = View( new ListModel{ ViewType=viewType.Value, OrderBy=orderBy.Value, NoLayout=noLayout, Items=rv.Result } );
			logHelper.AddLogMessage( $"ItemsList: END" );
			return result;
		}
		public struct ListModel
		{
			public ViewTypes	ViewType;
			public OrderBys		OrderBy;
			public bool			NoLayout;
			public DTOs.Item[]	Items;
		}

		[HttpGet( Routes.ItemDetails )]
		public async Task<IActionResult> Details(string itemCode)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemDetails: START: {nameof(itemCode)}:{itemCode}" );

			var rv = await (new Services.ItemController( DataContext, PageHelper )).Query( itemCode:itemCode, orderBy:null );
			if(! rv.Success )
				throw new Utils.TTTException( rv.ErrorMessage );
			if( rv.Result.Length == 0 )
				return NotFound();
			Utils.Assert( rv.Result.Length == 1, this, $"Invalid result count:{rv.Result.Length} ; expected 1" );
			var item = rv.Result[0];

			var result = View( item );
			logHelper.AddLogMessage( $"ItemDetails: END" );
			return result;
		}
	}
}
