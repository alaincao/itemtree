
using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

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
