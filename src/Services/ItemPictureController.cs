using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class ItemPictureController : Controller
	{
		internal const double		ScaleHToW			= (double)1920/1200;  // 1.6
		internal const double		ScaleWToH			= (double)1200/1920;  // 0.625
		internal static ImageFormat	ResizedFormat		=> ImageFormat.Jpeg;
		internal const string		ResizedContentType	= "image/jpeg";
		internal const string		ResizedExtension	= ".jpg";

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public ItemPictureController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.ItemPictureGet )]
		public async Task<IActionResult> Download(string itemCode, int number, int? height=null, bool forDownload=false)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemPicDownload START ; itemCode:'{itemCode}' ; number:{number} ; height:{height} ; forDownload:{forDownload}" );

			if( string.IsNullOrWhiteSpace(itemCode) )
				return NotFound();
			if( number <= 0 )
				return NotFound();
			if( (height != null) && (height.Value < 10) )  // Minimum height
				return NotFound();
			var dc = DataContext;

			logHelper.AddLogMessage( $"ItemPicDownload: Try retreive item" );
			var item = await dc.Items.Where( v=>v.Code == itemCode ).Select( v=>new{ v.ID, v.Active } ).SingleOrDefaultAsync();
			if( item == null )
				return NotFound();
			if( (item.Active == false) && (PageHelper.IsAutenticated == false) )
				return NotFound();

			var askedType = (height == null) ? Models.ItemPicture.Type_Original : ( Models.ItemPicture.Type_HeightPrefix + height.Value );
			Models.ItemPicture picture;
			{
				var tryType = askedType;
			TRY_RETREIVE:
				logHelper.AddLogMessage( $"ItemPicDownload: Try retreive with type:'{tryType}' and itemID:{item.ID}" );
				picture = await dc.ItemPictures	.Where( v=>v.ItemID == item.ID )
												.Where( v=>v.Number == number )
												.Where( v=>v.Type == tryType )
												.SingleOrDefaultAsync();
				if( picture == null )
				{
					if( tryType == Models.ItemPicture.Type_Original )
						// Original not found
						return NotFound();
					// Retry with original version
					tryType = Models.ItemPicture.Type_Original;
					goto TRY_RETREIVE;
				}
				else if( tryType == askedType )
				{
					// Found what I asked for => Skip resizing
					goto FOUND;
				}
				else
				{
					// Found but not the right size => continue with resizing
				}
			}
			// Resize required

			logHelper.AddLogMessage( $"ItemPicDownload: Crop/resize" );
			string newImageString;
			{
				Utils.Assert( (height != null) && (height.Value > 0), this, "Logic error: The 'height' is supposed to be set here" );

				Bitmap originalBitmap;
				{
					var bytes = Convert.FromBase64String( picture.Content );
					var ms = new System.IO.MemoryStream( bytes );  // NB: Do NOT dispose the stream ; The constructor below does NOT load the image entirely. ; Counting on the garbage collector here ...
					originalBitmap = new Bitmap( ms );
				}

				using( var ms = new System.IO.MemoryStream() )
				{
					var newImage = CropResize( originalBitmap, destinationHeight:(double)height.Value, scaleHToW:ScaleHToW, scaleWToH:ScaleWToH );
					newImage.Save( ms, ResizedFormat );
					var bytes = ms.ToArray();
					newImageString = Convert.ToBase64String( bytes );
				}
			}

			logHelper.AddLogMessage( $"ItemPicDownload: Save resized to database" );
			{
				Utils.Assert( askedType != Models.ItemPicture.Type_Original, this, "Logic error: Not supposed to save the original picture here ..." );
				// nb: reassign 'picture' variable with the final one
				picture = new Models.ItemPicture{	ItemID	= picture.ItemID,
													Number	= number,
													Type	= askedType,
													Content	= newImageString };
				try
				{
					dc.ItemPictures.Add( picture );
					await dc.SaveChangesAsync();
				}
				catch( System.Exception ex )
				{
					// Possible race-condition: 2 requests accessing the same resized image not yet in the database => both trying to save it resized at the same time
					// => The first to save it wins & the second with violate the UNIQUE constraint on the table
					// Happens when uploading multiple images at the same time => The page tries to redownload them more than once
					// => Discard exception
					Utils.Fail( this, $"Failed to save resized image ({ex.GetType().FullName}): {ex.Message}" );
				}
			}

		FOUND:
			// Here, 'picture' contains the output content

			logHelper.AddLogMessage( $"ItemPicDownload: Create output content" );
			var outputBytes = Convert.FromBase64String( picture.Content );

			string fileExtension;
			string contentType;
			{
				if( askedType != Models.ItemPicture.Type_Original )
				{
					// Resized images are always saved using the same format
					contentType = ResizedContentType;
					fileExtension = ResizedExtension;
				}
				else
				{
					logHelper.AddLogMessage( $"ItemPicDownload: Create image bitmap to determine the uploaded format" );
					var ms = new System.IO.MemoryStream( outputBytes );  // NB: Do NOT dispose the stream ; The constructor below does NOT load the image entirely. ; Counting on the garbage collector here ...
					var bitmap = new Bitmap( ms );

					var guid = bitmap.RawFormat.Guid;
					if( guid == ImageFormat.Bmp.Guid )
					{
						contentType = "image/x-ms-bmp";
						fileExtension = ".bmp";
					}
					else if( guid == ImageFormat.Jpeg.Guid )
					{
						contentType = "image/jpeg";
						fileExtension = ".jpg";
					}
					else if( guid == ImageFormat.Gif.Guid )
					{
						contentType = "image/gif";
						fileExtension = ".hif";
					}
					else if( guid == ImageFormat.Png.Guid )
					{
						contentType = "image/png";
						fileExtension = ".png";
					}
					else  // WTF is this ???
					{
						contentType = "application/octet-stream";
						fileExtension = "";
					}
				}
			}
			var fileName = (height == null) ? $"{itemCode}.{number}{fileExtension}" : $"{itemCode}.{number}.x{height}{fileExtension}";

			logHelper.AddLogMessage( $"ItemPicDownload: Output using contentType:'{contentType}' ; fileName:'{fileName}'" );
			if( forDownload )
				return File( new System.IO.MemoryStream(outputBytes), contentType, fileName );
			else
				return File( new System.IO.MemoryStream(outputBytes), contentType );
		}

		internal static Bitmap CropResize(Bitmap image, double destinationHeight, double scaleHToW, double scaleWToH)
		{
			Utils.Assert( destinationHeight > 0, typeof(ItemPictureController), "Invalid parameter 'destinationHeight'" );

			double destinationWidth = destinationHeight * scaleHToW;
			double sourceWidth = image.Width;
			double sourceHeight = image.Height;

			// Crop image
			{
				double sourceScaleWToH = sourceHeight / sourceWidth;
				int cropWidth, cropHeight;
				if( sourceScaleWToH == scaleWToH )
				{
					// No crop needed
					goto NO_CROP;
				}
				else if( sourceScaleWToH > scaleWToH )
				{
					// Crop on height
					cropWidth = (int)sourceWidth;
					cropHeight = (int)(sourceWidth * scaleWToH );
					if( cropHeight == sourceHeight )
						// Nothing changes after math roundings ...
						goto NO_CROP;
					Utils.Assert( cropHeight < sourceHeight, typeof(ItemPictureController), "Logic error: Cropped is supposed to be smaller than source ..." );
				}
				else // sourceRatioHW < RatioHW
				{
					// Crop on width
					cropHeight = (int)sourceHeight;
					cropWidth = (int)(sourceHeight*scaleHToW);
					if( cropWidth == sourceWidth )
						// Nothing changes after math roundings ...
						goto NO_CROP;
					Utils.Assert( cropWidth < sourceWidth, typeof(ItemPictureController), "Logic error: Cropped is supposed to be smaller than source ..." );
				}

				// Replace 'image' with the cropped one
				var cropRect = new Rectangle(	x:		(int)( (sourceWidth - cropWidth) / 2 ),
												y:		(int)( (sourceHeight - cropHeight) / 2 ),
												width:	cropWidth,
												height:	cropHeight );
				image = image.Clone( cropRect, image.PixelFormat );
			}
		NO_CROP:

			// Resized image
			if( (image.Height == destinationHeight) && (image.Width == destinationWidth) )
			{
				// No resize needed
			}
			else
			{
				image = new Bitmap( image, (int)destinationWidth, (int)destinationHeight );
			}

			return image;
		}
	}
}