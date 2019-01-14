
using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class BlogController : Views.BaseController
	{
		private const double		ScaleHToW			= ItemPictureController.ScaleHToW;
		private const double		ScaleWToH			= ItemPictureController.ScaleWToH;
		private const int			DestinationHeight	= 650;
		private static ImageFormat	ResizedFormat		=> ItemPictureController.ResizedFormat;

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public BlogController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.BlogListApi )]
		[HttpPost( Routes.BlogListApi )]
		public async Task<Utils.TTTServiceResult<DTOs.BlogPost[]>> List(bool includeImages=true, bool includeInactives=false, int? id=null, int? skipToID=null, int? skip=null, int? take=null)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"BlogList: START: {nameof(includeImages)}:{includeImages} ; {nameof(includeInactives)}:{includeInactives} ; {nameof(id)}:'{id}' ; {nameof(skip)}:'{skip}' ; {nameof(take)}:'{take}'" );
				var dc = DataContext;

				var queryAllOrdered = dc.BlogPosts.AsQueryable();
				{
					if(! includeImages )
						queryAllOrdered = Models.BlogPost.UnselectImage( queryAllOrdered );

					if( includeInactives && PageHelper.IsAuthenticated )
						{/*NOOP*/}
					else
						queryAllOrdered = queryAllOrdered.Where( v=>v.Active );

					queryAllOrdered = queryAllOrdered.OrderByDescending( v=>v.Date ).ThenByDescending( v=>v.ID );
				}

				logHelper.AddLogMessage( $"BlogList: Retreive all IDs" );
				int[] allIds = await queryAllOrdered.Select( v=>v.ID ).ToArrayAsync();

				logHelper.AddLogMessage( $"BlogList: Retreive rows" );
				Models.BlogPost[] models;
				{
					var rowsQuery = queryAllOrdered;
					if( id != null )
						rowsQuery = rowsQuery.Where( v=>v.ID == id.Value );
					if( skipToID != null )
					{
						var idx = Array.IndexOf( allIds, skipToID.Value );
						if( idx < 0 )
							throw new Utils.TTTException( $"Invalid value '{skipToID}' for parameter {nameof(skipToID)}" );
						rowsQuery = rowsQuery.Skip( idx + 1 );
					}
					if( skip != null )
						rowsQuery = rowsQuery.Skip( skip.Value );
					if( take != null )
						rowsQuery = rowsQuery.Take( take.Value );

					models = await rowsQuery.ToArrayAsync();
				}

				logHelper.AddLogMessage( $"BlogList: Convert to DTOs" );
				Func<int,string> mkUrl = (idx)=>
					{
						if( idx < 0 )
							return null;
						if( idx >= allIds.Length )
							return null;
						var url = Views.Blog.BlogController.CreateUrlDetails( PageHelper.CurrentLanguage, allIds[idx] );
						return PageHelper.ResolveRoute( url );
					};
				Func<int,string> mkEditUrl = (idx)=>
					{
						var url = Views.Blog.BlogController.CreateUrlEdit( allIds[idx] );
						return PageHelper.ResolveRoute( url );
					};
				var imgNotFound = $"<img src=\"{PageHelper.ResolveRoute(wwwroot.ImgNotFound)}\"/>";
				var dtos = models.Select( model=>
									{
										var idx = Array.IndexOf( allIds, model.ID );
										var idxPrevious = (idx < 0) ? -1 : (idx + 1);
										var idxNext = idx - 1;
										var dto = new DTOs.BlogPost( model )
											{
												Url			= mkUrl( idx ),
												UrlEdit		= mkEditUrl( idx ),
												UrlPrevious	= mkUrl( idxPrevious ),
												UrlNext		= mkUrl( idxNext ),
											};
										if( dto.ImageHtml == null )
											dto.ImageHtml = imgNotFound;
										return dto;
									} )
								.ToArray();

				logHelper.AddLogMessage( $"BlogList: END" );
				return new Utils.TTTServiceResult<DTOs.BlogPost[]>( PageHelper ){ Result=dtos };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.BlogPost[]>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.BlogSaveApi )]
		/// <return>The id of the saved blog post</return>
		public async Task<Utils.TTTServiceResult<int>> Save([FromBody]DTOs.BlogPost post)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				if( post == null )
					throw new ArgumentException( $"Missing parameter '{nameof(post)}" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"BlogSave: START: {nameof(post.ID)}:'{post.ID}'" );

				var dc = DataContext;
				var isAdding = (post.ID == null);

				Models.BlogPost model;
				if( isAdding )
				{
					logHelper.AddLogMessage( $"BlogSave: Adding a new one" );
					model = new Models.BlogPost();
					dc.BlogPosts.Add( model );
				}
				else
				{
					logHelper.AddLogMessage( $"BlogSave: Retreive post from database" );
					model = await dc.BlogPosts.Where( v=>v.ID == post.ID.Value ).SingleAsync();
				}
				logHelper.AddLogMessage( $"BlogSave: Copy fields" );
				post.ToModel( model );

				logHelper.AddLogMessage( $"BlogSave: Save changes to database" );
				await dc.SaveChangesAsync();

				logHelper.AddLogMessage( $"BlogSave: END: {nameof(model.ID)}:'{model.ID}'" );
				return new Utils.TTTServiceResult<int>( PageHelper ){ Result=model.ID };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<int>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.BlogDeleteApi )]
		public async Task<Utils.TTTServiceResult> Delete(int id)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"BlogDelete: START: {nameof(id)}:'{id}'" );

				if( id <= 0 )
					throw new ArgumentException( $"Missing parameter '{nameof(id)}'" );

				var post = new Models.BlogPost{ ID=id };
				var dc = DataContext;
				dc.BlogPosts.Attach( post );
				dc.BlogPosts.Remove( post );

				logHelper.AddLogMessage( $"BlogDelete: Save to database" );
				await dc.SaveChangesAsync();

				logHelper.AddLogMessage( $"BlogDelete: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.BlogPictureUpload )]
		public async Task<Utils.TTTServiceResult<string>> UploadPicture(Microsoft.AspNetCore.Http.IFormFile file)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"BlogUploadPicture: START" );

				logHelper.AddLogMessage( $"BlogUploadPicture: Get image content" );
				var imageStream = new System.IO.MemoryStream();
				await file.CopyToAsync( imageStream );  // Performance: Using 'await CopyToAsync()' instead of directly giving the stream to 'new Bitmap()' to use async/await

				logHelper.AddLogMessage( $"BlogUploadPicture: Crop/resize" );
				var bitmap = new System.Drawing.Bitmap( imageStream );
				var newImage = ItemPictureController.CropResize( bitmap, destinationHeight:(double)DestinationHeight, scaleHToW:ScaleHToW, scaleWToH:ScaleWToH );
				string newImageString;
				using( var ms = new System.IO.MemoryStream() )
				{
					newImage.Save( ms, ResizedFormat );
					var newImageBytes = ms.ToArray();
					newImageString = Convert.ToBase64String( newImageBytes );
				}

				var tag = "<img src=\"data:image/jpeg;base64," + newImageString + "\"/>";

				logHelper.AddLogMessage( $"BlogUploadPicture: END" );
				return new Utils.TTTServiceResult<string>( PageHelper ){ Result=tag };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string>.LogAndNew( PageHelper, ex );
			}
		}
	}
}
