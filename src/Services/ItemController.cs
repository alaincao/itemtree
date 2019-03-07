
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	using SortingFields = Views.ItemTTTController.SortingFields;

	public class ItemController : Views.BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public ItemController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.GetUrlCode )]
		[HttpPost( Routes.GetUrlCode )]
		public string GetUrlCode(string originalCode)
		{
			return ItemController.GetUrlCodeS( originalCode ) ?? "";
		}
		internal static string GetUrlCodeS(string originalCode)
		{
			if( string.IsNullOrWhiteSpace(originalCode) )
				return null;

			var chars = originalCode.ToLower().ToCharArray();
			for( int i=0; i<chars.Length; ++i)
			{
				var c = chars[i];
				if( ( (c >= 'a') && (c <= 'z') )
				 || ( (c >= '0') && (c <= '9') ) )
					{}  // OK
				else
					chars[i] = '-';
			}
			string str = new String( chars );
			for( var newStr = str.Replace("--", "-") ; newStr.Length != str.Length; newStr = str.Replace("--", "-") )
				str =  newStr;
			return str;
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
			if(! PageHelper.IsAuthenticated )
				query = query.Where( v=>v.Active == true );
			switch( sortingField )
			{
				case null: break;
				case SortingFields.name:		query = query.OrderBy( v=>v.Name );				break;
				case SortingFields.name_desc:	query = query.OrderByDescending( v=>v.Name );	break;
				case SortingFields.price:		query = query.OrderBy( v=>v.Price );			break;
				case SortingFields.price_desc:	query = query.OrderByDescending( v=>v.Price );	break;
				default:
					throw new NotImplementedException( $"Invalid value '{sortingField}' for parameter '{nameof(sortingField)}'" );
			}
			var rows = await query.ToAsyncEnumerable()
										.Select( v=>new{ ItemID=v.ID, v.Features, Item=new DTOs.Item(v) } )
										.ToArray();
			if( rows.Length == 0 )
				// No need to continue ...
				goto EXIT;

			logHelper.AddLogMessage( $"ItemQuery: Retreive pictures" );
			var picturesRv = await (new ItemPictureController(dc, PageHelper)).List( itemCode:itemCode );
			if(! picturesRv.Success )
				return new Utils.TTTServiceResult<DTOs.Item[]>( PageHelper, picturesRv.ErrorMessage );
			var picturesDict = picturesRv.Result.GroupBy( v=>v.ItemCode ).ToDictionary( v=>v.Key, v=>v.ToArray() );

			logHelper.AddLogMessage( $"ItemQuery: Complete fields for '{rows.Length}' items" );
			var allLanguages = Enum.GetValues(typeof(Languages)).Cast<Languages>().ToArray();
			var allFeatures = new HashSet<string>();
			foreach( var row in rows )
			{
				row.Item.DetailsUrls	= allLanguages.ToDictionary( v=>v, l=>PageHelper.ResolveRoute(Views.ItemTTTController.CreateUrlDetails(l, row.Item.Code)) );
				row.Item.Pictures		= picturesDict.TryGet( row.Item.Code ) ?? new DTOs.ItemPicture[]{};

				if( row.Features == null )
				{
					row.Item.Features = new DTOs.Translation[]{};
				}
				else
				{
					var strList = row.Features.JSONDeserialize<string[]>();
					row.Item.Features = strList.Select( v=>new DTOs.Translation{ EN=v } ).ToArray();
					allFeatures.UnionWith( strList );
				}
			}

			if( allFeatures.Count > 0 )
			{
				logHelper.AddLogMessage( $"ItemQuery: Retreive features translations" );
				var translations = await dc.Translations.Where( v=>v.TypeString == ""+Services.TranslationController.Types.Feature )
														.Where( v=>allFeatures.Contains(v.TranslationEN) )
														.ToDictionaryAsync( v=>v.TranslationEN, v=>v );
				logHelper.AddLogMessage( $"ItemQuery: Translate features using {translations.Count} translations" );
				foreach( var row in rows )
				{
					for( var i=0; i<row.Item.Features.Length; ++i )
					{
						if( translations.TryGetValue(row.Item.Features[i].EN, out var translation) )
						{
							// Translation available
							row.Item.Features[i].FR = translation.TranslationFR;
							row.Item.Features[i].NL = translation.TranslationNL;
						}
						else
						{
							// Translation not available => Use EN for all
							row.Item.Features[i].FR = row.Item.Features[i].EN;
							row.Item.Features[i].NL = row.Item.Features[i].EN;
						}
					}
				}
			}

		EXIT:
			logHelper.AddLogMessage( $"ItemQuery: END" );
			return new Utils.TTTServiceResult<DTOs.Item[]>( PageHelper ){ Result=rows.Select(v=>v.Item).ToArray() };
		}

		[HttpPost( Routes.ItemSave )]
		/// <return>The code of the saved item</return>
		public async Task<Utils.TTTServiceResult<string>> Save([FromBody]SaveRequest request)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogLines( $"ItemSave: START: {request.JSONStringify(indented:true)}" );

				logHelper.AddLogMessage( $"ItemSave: Open transaction" );
				var dc = DataContext;
				Models.Item item;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					if( request.OriginalCode == null )
					{
						logHelper.AddLogMessage( $"ItemSave: Create new" );
						item = new Models.Item{ MainImageNumber=1 };
						dc.Items.Add( item );
					}
					else
					{
						logHelper.AddLogMessage( $"ItemSave: Retreive from database" );
						item = await dc.Items.Where( v=>v.Code == request.OriginalCode ).SingleAsync();
						logHelper.AddLogLines( $"ItemSave: {item.JSONStringify(indented:true)}" );
					}

					logHelper.AddLogMessage( $"ItemSave: Validate and update fields" );
					request.Item.ToModel( logHelper, item );
					logHelper.AddLogLines( $"ItemSave: {item.JSONStringify(indented:true)}" );

					logHelper.AddLogLines( $"ItemSave: Execute SQL" );
					var rc = await dc.SaveChangesAsync();
					logHelper.AddLogMessage( $"ItemSave: Affected: {rc}" );
					Utils.Assert( item.ID > 0, this, "'SaveChangesAsync()' did not set the 'item.ID'" );

					logHelper.AddLogMessage( $"ItemSave: Commit transaction" );
					transaction.Commit();
				}

				logHelper.AddLogMessage( $"ItemSave: END" );
				return new Utils.TTTServiceResult<string>( PageHelper ){ Result = item.Code };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string>.LogAndNew( PageHelper, ex );
			}
		}
		public class SaveRequest
		{
			public string		OriginalCode	{ get; set; }
			public DTOs.Item	Item			{ get; set; }
		}

		[HttpPost( Routes.ItemDelete )]
		public async Task<Utils.TTTServiceResult> Delete(string itemCode)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogLines( $"ItemDelete: START: {itemCode}" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					return new Utils.TTTServiceResult( PageHelper , $"Parameter {nameof(itemCode)} is missing" );

				logHelper.AddLogMessage( $"ItemDelete: Retreive from database" );
				var dc = DataContext;
				var item = await dc.Items.Where( v=>v.Code == itemCode ).SingleAsync();

				logHelper.AddLogLines( $"ItemDelete: Item: {item.JSONStringify(indented:true)}" );
				dc.Items.Remove( item );
				dc.SaveChanges();

				logHelper.AddLogMessage( $"ItemDelete: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}
	}
}
