
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public class BlogController : BaseController
	{
		private const string	ListPartial				= Startup.ViewsLocation+"Blog/List_Partial.cshtml";
		private const int		PostsNumberInitial		= 10;
		private const int		PostsNumberIncrement	= 5;

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		internal static string CreateUrlDetails(Languages language, int blogPostID)
		{
			var url = Routes.BlogDetails
								.Replace( Language.RouteParameter, ""+language )
								.Replace( "{id}", ""+blogPostID );
			return url;
		}

		public BlogController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.BlogList )]
		public async Task<IActionResult> List(bool noLayout=false, bool includeImages=false, bool includeInactives=false, int? fromID=null, int? take=null)
		{
			if( take == null )
				take = PostsNumberInitial;

			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogList: START: {nameof(noLayout)}:{noLayout} ; {nameof(includeImages)}:{includeImages} ; {nameof(includeInactives)}:{includeInactives} ; {nameof(fromID)}:{fromID} ; {nameof(take)}:{take}" );

			var rv = await (new Services.BlogController( DataContext, PageHelper )).List( includeImages:includeImages, includeInactives:includeInactives, fromID:fromID, take:take );

			var model = new ListModel{	BlogPosts				= rv.Result ?? (new DTOs.BlogPost[]{}),
										PartialView				= ListPartial,
										PostsNumberIncrement	= PostsNumberIncrement };

			ViewResult result;
			if( noLayout )
				result = View( ListPartial, model );
			else
				result = View( model );
			logHelper.AddLogMessage( $"BlogList: END" );
			return result;
		}
		public struct ListModel
		{
			public DTOs.BlogPost[]	BlogPosts;
			public string			PartialView;
			public int				PostsNumberIncrement;
		}

		[HttpGet( Routes.BlogDetails )]
		public async Task<IActionResult> Details(int id)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogDetails: START: {nameof(id)}:{id}" );

			var includeInactives = PageHelper.IsAuthenticated;
			var rv = await (new Services.BlogController( DataContext, PageHelper )).List( id:id, includeInactives:includeInactives );
			if(! rv.Success )
				throw NewUnexpectedException( rv.ErrorMessage );
			if( rv.Result.Length == 0 )
				return ObjectNotFound();
			Utils.Assert( rv.Result.Length == 1, this, $"Invalid result count:{rv.Result.Length} ; expected 1" );
			var item = rv.Result[0];

			var result = View( item );
			logHelper.AddLogMessage( $"BlogDetails: END" );
			return result;
		}

		[HttpGet( Routes.BlogEdit )]
		public IActionResult Edit()
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogEdit: START" );

			logHelper.AddLogMessage( $"BlogEdit: END" );
			return View();
		}
	}
}
