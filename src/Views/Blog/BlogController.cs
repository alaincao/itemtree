
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views.Blog
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
			Utils.Assert( blogPostID > 0, typeof(BlogController), $"Missing parameter '{nameof(blogPostID)}'" );
			var url = Routes.BlogDetails
								.Replace( Routes.LangParameter, ""+language )
								.Replace( Routes.ItemIDParameter, ""+blogPostID );
			return url;
		}
		internal static string CreateUrlEdit(int blogPostID)
		{
			Utils.Assert( blogPostID > 0, typeof(BlogController), $"Missing parameter '{nameof(blogPostID)}'" );
			var url = Routes.BlogEdit
								.Replace( Routes.ItemIDParameter, ""+blogPostID );
			return url;
		}

		public BlogController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.BlogList )]
		public async Task<IActionResult> List(bool noLayout=false, bool includeImages=false, bool includeInactives=false, int? skipToID=null, int? take=null)
		{
			if( take == null )
				take = PostsNumberInitial;

			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogList: START: {nameof(noLayout)}:{noLayout} ; {nameof(includeImages)}:{includeImages} ; {nameof(includeInactives)}:{includeInactives} ; {nameof(skipToID)}:{skipToID} ; {nameof(take)}:{take}" );

			var rv = await (new Services.BlogController( DataContext, PageHelper )).List( includeImages:includeImages, includeInactives:includeInactives, skipToID:skipToID, take:take );

			var model = new ListModel{	PartialView				= ListPartial,
										BlogPosts				= rv.Result ?? (new DTOs.BlogPost[]{}),
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
			public string			PartialView;
			public DTOs.BlogPost[]	BlogPosts;
			public int				PostsNumberIncrement;
		}

		[HttpGet( Routes.BlogDetails )]
		public Task<IActionResult> Details(int id)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogDetails: {nameof(id)}:{id}" );
			return ShowItem( id );
		}

		[HttpGet( Routes.BlogAdd )]
		[HttpGet( Routes.BlogEdit )]
		public async Task<IActionResult> Edit(int? id)
		{
			if(! PageHelper.IsAuthenticated )
				return NotAuthenticated();

			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogEdit: {nameof(id)}:{id}" );

			if( id == null )
				// Add
				return View( (DTOs.BlogPost)null );
			else
				// Edit
				return await ShowItem( id.Value );
		}

		private async Task<IActionResult> ShowItem(int id)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"BlogShowItem: START: {nameof(id)}:{id}" );

			var includeInactives = PageHelper.IsAuthenticated;
			var rv = await (new Services.BlogController( DataContext, PageHelper )).List( id:id, includeInactives:includeInactives );
			if(! rv.Success )
				throw NewUnexpectedException( rv.ErrorMessage );
			if( rv.Result.Length == 0 )
				return ObjectNotFound();
			Utils.Assert( rv.Result.Length == 1, this, $"Invalid result count:{rv.Result.Length} ; expected 1" );
			var item = rv.Result[0];

			logHelper.AddLogMessage( $"BlogShowItem: END" );
			return View( item );
		}
	}
}
