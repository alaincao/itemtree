
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class TranslationController : Views.BaseController
	{
		public enum Types
		{
			Undefined			= 0,
			Feature,
		}

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public TranslationController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.AutoComplete )]
		[HttpPost( Routes.AutoComplete )]
		public async Task<Utils.TTTServiceResult<string[]>> AutoCompleteResolve(Types type, string searchString=null, bool includeTranslations=true)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					// As of now, this is only useful for admins ...
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogLines( $"AutoCompleteResolve: START: {nameof(type)}:'{type}' ; {nameof(searchString)}:'{searchString}' ; {nameof(includeTranslations)}:{includeTranslations}" );
				var dc = DataContext;

				IEnumerable<string> stringsList = null;
				IEnumerable<string> jsonLists = null;
				switch( type )
				{
					case Types.Feature:
						jsonLists = await dc.Items.Select( v=>v.Features ).ToArrayAsync();
						break;
					default:
						throw new NotImplementedException( $"Unknown value '{type}' for parameter {nameof(type)}" );
				}

				if( jsonLists != null )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Using JSON lists" );

					if( stringsList == null )
						stringsList = new string[]{};
					foreach( var jsonString in jsonLists )
					{
						var strs = jsonString.JSONDeserialize<string[]>();
						stringsList = stringsList.Concat( strs );
					}
				}

				if( includeTranslations )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Using translations" );
					var strType = ""+type;
					var trans = dc.Translations.Where( v=>v.TypeString == strType ).Select( v=>v.TranslationEN );
					stringsList = stringsList.Concat( trans );
				}

				logHelper.AddLogLines( $"AutoCompleteResolve: Distinct" );
				stringsList = stringsList.Distinct();

				if(! string.IsNullOrEmpty(searchString) )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Applying {nameof(searchString)}" );
					searchString = searchString.ToLower();
					stringsList = stringsList.Where( v=>v.ToLower().Contains(searchString) );
				}

				logHelper.AddLogLines( $"AutoCompleteResolve: END" );
				return new Utils.TTTServiceResult<string[]>( PageHelper ){ Result=stringsList.ToArray() };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string[]>.LogAndNew( PageHelper, ex );
			}
		}
	}
}
