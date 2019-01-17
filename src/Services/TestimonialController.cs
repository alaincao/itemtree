
using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class TestimonialController : Views.BaseController
	{
		internal const double		ScaleHToW			= 1;
		internal const double		ScaleWToH			= 1;
		private const int			DestinationHeight	= 650;
		private static ImageFormat	ResizedFormat		=> ItemPictureController.ResizedFormat;

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public TestimonialController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.TestimListApi )]
		[HttpPost( Routes.TestimListApi )]
		public async Task<Utils.TTTServiceResult<DTOs.Testimonial[]>> List(bool includeImages=true, bool includeInactives=true, int? id=null)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"TestimonialList: START: {nameof(includeImages)}:{includeImages} ; {nameof(includeInactives)}:'{includeInactives}' ; {nameof(id)}:'{id}'" );
				var dc = DataContext;

				var query = dc.Testimonials.AsQueryable();

				if(! includeImages )
					query = Models.Testimonial.UnselectImage( query );
				if( includeInactives && PageHelper.IsAuthenticated )
					{/*NOOP*/}
				else
					query = query.Where( v=>v.Active );
				if( id != null )
					query = query.Where( v=>v.ID == id.Value );

				logHelper.AddLogMessage( $"TestimonialList: Retreive rows" );
				var imgNotFound = PageHelper.ResolveRoute( wwwroot.ImgNotFound );
				var dtos = await query	.OrderByDescending( v=>v.ID )
										.ToAsyncEnumerable()
										.Select( v=>
											{
												var dto = new DTOs.Testimonial( v );
												if( dto.ImageData == null )
													dto.ImageData = imgNotFound;
												return dto;
											} )
										.ToArray();

				logHelper.AddLogMessage( $"TestimonialList: END" );
				return new Utils.TTTServiceResult<DTOs.Testimonial[]>( PageHelper ){ Result=dtos };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.Testimonial[]>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.TestimPictUpload )]
		public async Task<Utils.TTTServiceResult<string>> UploadPicture(Microsoft.AspNetCore.Http.IFormFile file)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"TestimonialUploadPicture: START" );

				logHelper.AddLogMessage( $"TestimonialUploadPicture: Get image content" );
				var imageStream = new System.IO.MemoryStream();
				await file.CopyToAsync( imageStream );  // Performance: Using 'await CopyToAsync()' instead of directly giving the stream to 'new Bitmap()' to use async/await

				logHelper.AddLogMessage( $"TestimonialUploadPicture: Crop/resize" );
				var bitmap = new System.Drawing.Bitmap( imageStream );
				var newImage = ItemPictureController.CropResize( bitmap, destinationHeight:(double)DestinationHeight, scaleHToW:ScaleHToW, scaleWToH:ScaleWToH );
				string newImageString;
				using( var ms = new System.IO.MemoryStream() )
				{
					newImage.Save( ms, ResizedFormat );
					var newImageBytes = ms.ToArray();
					newImageString = Convert.ToBase64String( newImageBytes );
				}

				var imageData = "data:image/jpeg;base64," + newImageString;

				logHelper.AddLogMessage( $"TestimonialUploadPicture: END" );
				return new Utils.TTTServiceResult<string>( PageHelper ){ Result=imageData };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string>.LogAndNew( PageHelper, ex );
			}
		}
	}
}
