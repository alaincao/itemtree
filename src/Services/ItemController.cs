
using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	using SortingFields = Views.ItemTTTController.SortingFields;

	public class ItemController : Controller
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public ItemController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.ItemsListApi )]
		[HttpPost( Routes.ItemsListApi )]
		public async Task<Utils.TTTServiceResult<DTOs.Item[]>> List(SortingFields? sortingField=null)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemsList: START: {nameof(sortingField)}:{sortingField}" );

				if( sortingField == null )
					// nb: APIs are using had-coded values to have consistent behaviour => Not checking cookie here
					sortingField = Views.ItemTTTController.SortingFieldDefault;

				var rv = await Query( itemCode:null, sortingField:sortingField.Value );

				logHelper.AddLogMessage( $"ItemsList: END" );
				return rv;
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.Item[]>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.ItemDetailsApi )]
		[HttpPost( Routes.ItemDetailsApi )]
		public async Task<Utils.TTTServiceResult<DTOs.Item>> Details(string itemCode)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemDetails: START: {nameof(itemCode)}:{itemCode}" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					return new Utils.TTTServiceResult<DTOs.Item>( PageHelper, $"Missing parameter {nameof(itemCode)}" );

				var rv = await Query( itemCode:itemCode, sortingField:null );
				if(! rv.Success )
					return new Utils.TTTServiceResult<DTOs.Item>( PageHelper, rv.ErrorMessage );
				if( rv.Result.Length == 0 )
					return new Utils.TTTServiceResult<DTOs.Item>( PageHelper, "Item not found" );

				Utils.Assert( rv.Result.Length == 1, this, $"Invalid result count: {rv.Result.Length} ; expected 1" );

				logHelper.AddLogMessage( $"ItemDetails: END" );
				return new Utils.TTTServiceResult<DTOs.Item>( PageHelper ){ Result=rv.Result[0] };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.Item>.LogAndNew( PageHelper, ex );
			}
		}

		internal async Task<Utils.TTTServiceResult<DTOs.Item[]>> Query(string itemCode, SortingFields? sortingField)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemQuery: START: {nameof(itemCode)}:{itemCode}, {nameof(sortingField)}:{sortingField}" );

			var dc = DataContext;

			logHelper.AddLogMessage( $"ItemQuery: Try retreive items" );
			var query = dc.Items.AsQueryable();
			if( itemCode != null )
				query = query.Where( v=>v.Code == itemCode );
			if(! PageHelper.IsAutenticated )
				query = query.Where( v=>v.Active == true );
			switch( sortingField )
			{
				case null: break;
				case SortingFields.name:	query = query.OrderBy( v=>v.Name );		break;
				case SortingFields.price:	query = query.OrderBy( v=>v.Price );	break;
				default:
					throw new NotImplementedException( $"Invalid value '{sortingField}' for parameter '{nameof(sortingField)}'" );
			}
			var items = await query.ToAsyncEnumerable()
										.Select( v=>new DTOs.Item(v) )
										.ToArray();
			if( items.Length == 0 )
				// No need to continue ...
				goto EXIT;

			logHelper.AddLogMessage( $"ItemQuery: Retreive pictures" );
			var picturesRv = await (new ItemPictureController(dc, PageHelper)).List( itemCode:itemCode );
			if(! picturesRv.Success )
				return new Utils.TTTServiceResult<DTOs.Item[]>( PageHelper, picturesRv.ErrorMessage );
			var picturesDict = picturesRv.Result.GroupBy( v=>v.ItemCode ).ToDictionary( v=>v.Key, v=>v.ToArray() );

			logHelper.AddLogMessage( $"ItemQuery: Complete fields for '{items.Length}' items" );
			var allLanguages = Enum.GetValues(typeof(Languages)).Cast<Languages>().ToArray();
			foreach( var item in items )
			{
				item.DetailsUrls	= allLanguages.ToDictionary( v=>v, l=>PageHelper.ResolveRoute(Views.ItemTTTController.CreateUrlDetails(l, item.Code)) );
				item.Pictures	= picturesDict.TryGet( item.Code ) ?? new DTOs.ItemPicture[]{};
			}

		EXIT:
			logHelper.AddLogMessage( $"ItemQuery: END" );
			return new Utils.TTTServiceResult<DTOs.Item[]>( PageHelper ){ Result=items };
		}
	}
}
