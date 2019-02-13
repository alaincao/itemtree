
using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class DynamicPageController : Views.BaseController
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public DynamicPageController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.DynamicPageGetApi )]
		[HttpPost( Routes.DynamicPageGetApi )]
		public async Task<Utils.TTTServiceResult<string>> Get(string itemCode, Languages language)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"DynamicPageGet: START: {nameof(itemCode)}:{itemCode} ; {nameof(language)}:{language}" );

				var dc = DataContext;

				logHelper.AddLogMessage( $"DynamicPageGet: Create query" );
				IQueryable<string> query;
				{
					var q = dc.DynamicPages.Where( v=>v.Code == itemCode );
					switch( language )
					{
						case Languages.en:	query = q.Select( v=>v.TranslationEN );	break;
						case Languages.fr:	query = q.Select( v=>v.TranslationFR );	break;
						case Languages.nl:	query = q.Select( v=>v.TranslationNL );	break;
						default:
							throw new NotImplementedException( $"Unkonwn value '{language}' for parameter '{nameof(language)}'" );
					}
				}

				logHelper.AddLogMessage( $"DynamicPageGet: Execute query" );
				var text = await query.SingleAsync();

				logHelper.AddLogLines( $"DynamicPageGet: END" );
				return new Utils.TTTServiceResult<string>( PageHelper ){ Result = text };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.DynamicPageUpdateApi )]
		[HttpPost( Routes.DynamicPageUpdateApi )]
		public async Task<Utils.TTTServiceResult<string>> Update(string itemCode, Languages language, string text, bool allowAdd=false)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"DynamicPageUpdate: START: {nameof(itemCode)}:{itemCode} ; {nameof(language)}:{language} ; {nameof(text)} length:{(text??"").Length} ; {nameof(allowAdd)}:{allowAdd}" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					throw new ArgumentException( $"Missing parameter '{nameof(itemCode)}'" );
				if( text == null )
					throw new ArgumentException( $"Missing parameter '{nameof(text)}'" );

				itemCode = ItemController.GetUrlCodeS( itemCode );
				logHelper.AddLogMessage( $"DynamicPageUpdate: Sanitized {nameof(itemCode)}: '{itemCode??"<null>"}'" );

				var dc = DataContext;

				logHelper.AddLogMessage( $"DynamicPageUpdate: Retreive from database" );
				Models.DynamicPage page;
				{
					// Performance: retreive only the ID field
					var query = dc.DynamicPages.Where( v=>v.Code == itemCode ).Select( v=>(int?)v.ID );
					int id;
					if( allowAdd )
						id = await query.SingleOrDefaultAsync() ?? 0;
					else
						id = await query.SingleAsync() ?? 0;

					page = new Models.DynamicPage{ ID=id, Code=itemCode, TranslationEN="", TranslationFR="", TranslationNL="" };

					if( id == 0 )
					{
						logHelper.AddLogMessage( $"DynamicPageUpdate: Add new" );
						dc.DynamicPages.Add( page );
					}
					else
					{
						logHelper.AddLogMessage( $"DynamicPageUpdate: Attach model to datacontext" );
						// nb: since the picture objects were not retreived as entities, they're not attached to the DataContext
						// => attach them as if they were retreived entities from the database
						// only updated fields should be updated to the database => since 'Content' is not updated (i.e. only 'Number' will), it will be left untouched
						dc.Entry( page ).State = EntityState.Unchanged;
					}
				}

				logHelper.AddLogMessage( $"DynamicPageUpdate: Save to database" );
				switch( language )
				{
					case Languages.en:	page.TranslationEN = text;	break;
					case Languages.fr:	page.TranslationFR = text;	break;
					case Languages.nl:	page.TranslationNL = text;	break;
					default:
						throw new NotImplementedException( $"Invalid value '{language}' for parameter '{nameof(language)}'" );
				}
				await DataContext.SaveChangesAsync();

				logHelper.AddLogLines( $"DynamicPageUpdate: END" );
				return new Utils.TTTServiceResult<string>( PageHelper ){ Result=itemCode };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string>.LogAndNew( PageHelper, ex );
			}
		}
	}
}
