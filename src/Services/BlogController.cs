
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
		public async Task<Utils.TTTServiceResult<DTOs.BlogPost[]>> List(bool includeImages=true, bool includeInactives=false, int? id=null, int? fromID=null, int? skip=null, int? take=null)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"BlogList: START: {nameof(includeImages)}:{includeImages} ; {nameof(id)}:'{id}' ; {nameof(skip)}:'{skip}' ; {nameof(take)}:'{take}'" );
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
					if( fromID != null )
					{
						var idx = Array.IndexOf( allIds, fromID.Value );
						if( idx < 0 )
							throw new Utils.TTTException( $"Invalid value '{fromID}' for parameter {nameof(fromID)}" );
						rowsQuery = rowsQuery.Skip( idx );
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
						var url = Views.BlogController.CreateUrlDetails( PageHelper.CurrentLanguage, allIds[idx] );
						return PageHelper.ResolveRoute( url );
					};
				var dtos = models.Select( model=>
									{
										var idx = Array.IndexOf( allIds, model.ID );
										var idxPrevious = (idx < 0) ? -1 : (idx + 1);
										var idxNext = idx - 1;
										var dto = new DTOs.BlogPost( model )
											{
												UrlPrevious	= mkUrl( idxPrevious ),
												UrlNext		= mkUrl( idxNext ),
											};
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
