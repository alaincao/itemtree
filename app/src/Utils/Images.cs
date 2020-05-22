
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ItemTTT
{
	public static partial class Utils
	{
		internal static class Images
		{
			internal static async Task<string> GetImageBase64String(LogHelper logHelper, Microsoft.AspNetCore.Http.IFormFile file)
			{
				logHelper.AddLogMessage( $"{nameof(GetImageBase64String)}: Create base64 string" );
				(await Files.GetBase64String( logHelper, file )).R( out var base64, out var ms );

				logHelper.AddLogMessage( $"{nameof(GetImageBase64String)}: Test resize ({ms.Length} bytes)" );
				{
					var imageFull = System.Drawing.Image.FromStream( ms );
					var imageResized = (System.Drawing.Image)new System.Drawing.Bitmap( imageFull, 640, 480 );

					ms.Position = 0;  // Rewind stram
				}

				return base64;
			}

			internal static void GetImageType(byte[] bytes, out string contentType, out string fileExtension)
			{
				using( var ms = new System.IO.MemoryStream(bytes) )
				{
					var bitmap = new Bitmap( ms );
					GetImageType( bitmap, out contentType, out fileExtension );
				}
			}
			internal static void GetImageType(Bitmap bitmap, out string contentType, out string fileExtension)
			{
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
					fileExtension = ".gif";
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

			internal static Bitmap CropResize(Bitmap image, double destinationHeight, double scaleHToW, double scaleWToH)
			{
				Utils.Assert( destinationHeight > 0, nameof(CropResize), "Invalid parameter 'destinationHeight'" );

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
						Utils.Assert( cropHeight < sourceHeight, nameof(CropResize), "Logic error: Cropped is supposed to be smaller than source ..." );
					}
					else // sourceRatioHW < RatioHW
					{
						// Crop on width
						cropHeight = (int)sourceHeight;
						cropWidth = (int)(sourceHeight*scaleHToW);
						if( cropWidth == sourceWidth )
							// Nothing changes after math roundings ...
							goto NO_CROP;
						Utils.Assert( cropWidth < sourceWidth, nameof(CropResize), "Logic error: Cropped is supposed to be smaller than source ..." );
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
}
