using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class ItemPictureController : Views.BaseController
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

		private static string CreateUrlDownload(string itemCode, int? number=null, string numberTemplate=null, int? height=null, string heightTemplate=null, bool forDownload=false)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(itemCode), typeof(ItemPictureController), $"Missing parameter {nameof(itemCode)}" );

			if( number == null )
				Utils.Assert( numberTemplate == null, typeof(ItemPictureController), $"If {nameof(number)} is not specified, {nameof(numberTemplate)} should be specified" );
			else
				numberTemplate = ""+number;

			var parms = new List<string>();

			if( height != null )
				heightTemplate = ""+height.Value;
			if( heightTemplate != null )
				parms.Add( $"height={heightTemplate}" );

			if( forDownload )
				parms.Add( $"fordownload=true" );

			var strParms = string.Join( "&", parms );
			if( strParms.Length > 0 )
			strParms = $"?{strParms}";

			itemCode = WebUtility.UrlEncode( itemCode );
			var url = Routes.ItemPictureDownload
									.Replace( "{itemCode}", itemCode )
									.Replace( "{number}", numberTemplate )
							+ strParms;
			return url;
		}

		[HttpGet( Routes.ItemPictureList )]
		[HttpPost( Routes.ItemPictureList )]
		public async Task<Utils.TTTServiceResult<DTOs.ItemPicture[]>> List(string itemCode=null)
		{
			// nb: 'itemCode' is part of URL's path and so 'itemCode=null' should not be accessible from public URL (i.e. only used internally)
			//		i.e. slow to execute, but required for e.g. ItemsList

			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemPicList: START: '{nameof(itemCode)}':{itemCode}" );

			try
			{
				logHelper.AddLogMessage( $"ItemPicList: Try retreive pictures" );
				var dc = DataContext;
				var items = dc.Items.AsQueryable();
				if( itemCode != null )
					items = items.Where( v=>v.Code == itemCode );
				if(! PageHelper.IsAuthenticated )
					items = items.Where( v=>v.Active == true );
				var images = await (	from item in items
										join pict in dc.ItemPictures on item.ID equals pict.ItemID
										orderby item.Code, pict.Number
										select new{	item.Code,
													item.MainImageNumber,
													Picture = new DTOs.ItemPicture{ Number=pict.Number } } )
									.Distinct()
									.ToArrayAsync();

				logHelper.AddLogMessage( $"ItemPicList: Complete fields for '{images.Length}' images" );
				foreach( var image in images )
				{
					var code = image.Code;
					var number = image.Picture.Number;
					image.Picture.ItemCode		= code;
					image.Picture.IsMainImage	= (image.MainImageNumber == number);
					image.Picture.UrlOriginal	= PageHelper.ResolveRoute( CreateUrlDownload(code, number) );
					image.Picture.Url50			= PageHelper.ResolveRoute( CreateUrlDownload(code, number, height:50) );
					image.Picture.Url100		= PageHelper.ResolveRoute( CreateUrlDownload(code, number, height:100) );
					image.Picture.Url133		= PageHelper.ResolveRoute( CreateUrlDownload(code, number, height:133) );
					image.Picture.Url260		= PageHelper.ResolveRoute( CreateUrlDownload(code, number, height:260) );
				}

				var result = images.Select( v=>v.Picture ).ToArray();
				logHelper.AddLogMessage( $"ItemPicList END" );
				return new Utils.TTTServiceResult<DTOs.ItemPicture[]>( PageHelper ){ Result=result };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.ItemPicture[]>.LogAndNew( PageHelper, ex, result:new DTOs.ItemPicture[]{} );
			}
		}

		[HttpPost( Routes.ItemPictureUpload )]
		public async Task<Utils.TTTServiceResult<int>> Upload(string itemCode, Microsoft.AspNetCore.Http.IFormFile file)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemPicUpload: START '{itemCode}'" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					throw new ArgumentNullException( $"{nameof(itemCode)}" );

				logHelper.AddLogMessage( $"ItemPicUpload: Create MemoryStream" );
				var ms = new System.IO.MemoryStream();
				await file.CopyToAsync( ms );

				logHelper.AddLogMessage( $"ItemPicUpload: Test resize ({ms.Length} bytes)" );
				{
					var imageFull = System.Drawing.Image.FromStream( ms );
					var imageResized = (System.Drawing.Image)new System.Drawing.Bitmap( imageFull, 640, 480 );

					ms.Position = 0;  // Rewind stram
				}

				logHelper.AddLogMessage( $"ItemPicUpload: Create base64 string" );
				var imageString = Convert.ToBase64String( ms.GetBuffer() );

				logHelper.AddLogMessage( $"ItemPicUpload: Open transaction" );
				var dc = DataContext;
				int imageNumber;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					logHelper.AddLogMessage( $"ItemPicUpload: Get ItemID" );
					var itemID = await dc.Items.Where( v=>v.Code == itemCode ).Select( v=>(int?)v.ID ).SingleOrDefaultAsync() ?? -1;
					if( itemID <= 0 )
						throw new ArgumentException( $"Item code '{itemCode}' was not found" );

					logHelper.AddLogMessage( $"ItemPicUpload: Get the image number" );
					imageNumber = await dc.ItemPictures.Where( v=>v.ItemID == itemID ).Select( v=>(int?)v.Number ).MaxAsync() ?? 0;
					++ imageNumber;
					var isMainImage = (imageNumber == 1) ? true : false;

					logHelper.AddLogMessage( $"ItemPicUpload: Create picture object ; number:{imageNumber} ; string length:{imageString.Length}" );
					var obj = new Models.ItemPicture{	ItemID = itemID,
														Content = imageString,
														Number = imageNumber,
														Type = Models.ItemPicture.Type_Original };
					dc.ItemPictures.Add( obj );

					logHelper.AddLogMessage( $"ItemPicUpload: Write to database" );
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicUpload: Commit transaction" );
					transaction.Commit();
				}

				logHelper.AddLogMessage( $"ItemPicUpload: END" );
				return new Utils.TTTServiceResult<int>( PageHelper ){ Result=imageNumber };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<int>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.ItemPictureDownload )]
		public async Task<IActionResult> Download(string itemCode, int number, int? height=null, bool forDownload=false)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"ItemPicDownload: START: {nameof(itemCode)}:{itemCode} ; {nameof(number)}:{number} ; {nameof(height)}:{height} ; {nameof(forDownload)}:{forDownload}" );

			if( string.IsNullOrWhiteSpace(itemCode) )
				return InvalidParameters();
			if( number <= 0 )
				return InvalidParameters();
			if( (height != null) && (height.Value < 10) )  // Minimum height
				return InvalidParameters();
			var dc = DataContext;

			logHelper.AddLogMessage( $"ItemPicDownload: Try retreive item" );
			var item = await dc.Items.Where( v=>v.Code == itemCode ).Select( v=>new{ v.ID, v.Active } ).SingleOrDefaultAsync();
			if( item == null )
				return ObjectNotFound();
			if( (item.Active == false) && (PageHelper.IsAuthenticated == false) )
				return NotAuthenticated();

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
						return ObjectNotFound();
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

		[HttpGet( Routes.ItemPictureDelete )]
		[HttpPost( Routes.ItemPictureDelete )]
		public async Task<Utils.TTTServiceResult> Delete(string itemCode, int number)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemPicDelete: START {nameof(itemCode)}:'{itemCode}' ; {nameof(number)}:'{number}'" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					throw new ArgumentNullException( $"{nameof(itemCode)}" );
				if( number <= 0 )
					throw new ArgumentNullException( $"{nameof(number)}" );

				logHelper.AddLogMessage( $"ItemPicDelete: Open transaction" );
				var dc = DataContext;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					logHelper.AddLogMessage( $"ItemPicDelete: Get item" );
					var item = await dc.Items.Where( v=>v.Code == itemCode ).SingleAsync();

					logHelper.AddLogMessage( $"ItemPicDelete: Get pictures" );
					var images = await dc.ItemPictures.Where( v=>v.ItemID == item.ID ).OrderBy( v=>v.Number ).ToArrayAsync();

					logHelper.AddLogMessage( $"ItemPicDelete: Remove selected images" );
					dc.ItemPictures.RemoveRange( images.Where(v=>v.Number == number) );
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicDelete: Decrement image number of each images with number above the removed one (in incremental order)" );
					foreach( var image in images.Where(v=>v.Number > number) )
					{
						image.Number = image.Number - 1;
						await dc.SaveChangesAsync();
					}

					logHelper.AddLogMessage( $"ItemPicDelete: Update item's 'MainImageNumber' if required" );
					if( item.MainImageNumber == number )
						item.MainImageNumber = null;
					else if( item.MainImageNumber > number )
						item.MainImageNumber = item.MainImageNumber - 1;
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicDelete: Commit transaction" );
					transaction.Commit();
				}

				logHelper.AddLogMessage( $"ItemPicDelete: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.ItemPictureReorder )]
		[HttpPost( Routes.ItemPictureReorder )]
		public async Task<Utils.TTTServiceResult> Reorder(string itemCode, int number, int newNumber)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemPicReorder: START {nameof(itemCode)}:'{itemCode}' ; {nameof(number)}:'{number}' ; {nameof(newNumber)}:'{newNumber}'" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					throw new ArgumentNullException( $"{nameof(itemCode)}" );
				if( number <= 0 )
					throw new ArgumentNullException( $"{nameof(number)}" );
				if( newNumber <= 0 )
					throw new ArgumentNullException( $"{nameof(newNumber)}" );

				logHelper.AddLogMessage( $"ItemPicReorder: Open transaction" );
				var dc = DataContext;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					logHelper.AddLogMessage( $"ItemPicReorder: Get item" );
					var item = await dc.Items.Where( v=>v.Code == itemCode ).SingleAsync();
					var mainImageNumber = item.MainImageNumber;

					var allPictures = await dc.ItemPictures.Where( v=>v.ItemID == item.ID )
													// Performance: Retreive all fields except 'Content' (i.e. too heavy)
													.Select( v=>new Models.ItemPicture(){	ID		= v.ID,
																							ItemID	= v.ItemID,
																							Number	= v.Number,
																							Type	= v.Type,
																							Content	= null } )
													.ToArrayAsync();
					foreach( var pic in allPictures )
						// nb: since the picture objects were not retreived as entities, they're not attached to the DataContext
						// => attach them as if they were retreived entities from the database
						// only updated fields should be updated to the database => since 'Content' is not updated (i.e. only 'Number' will), it will be left untouched
						dc.Entry( pic ).State = EntityState.Unchanged;

					var maxPictureNumber = allPictures.Select( v=>v.Number ).Max();
					if( newNumber > maxPictureNumber )
					{
						newNumber = maxPictureNumber;
						logHelper.AddLogMessage( $"ItemPicReorder: Out of bound => Using {nameof(newNumber)}:'{newNumber}'" );
					}
					if( number == newNumber )
					{
						logHelper.AddLogMessage( $"ItemPicReorder: Nothing to do" );
						goto EXIT;
					}

					var zePictures = allPictures.Where( v=>v.Number == number ).ToArray();

					logHelper.AddLogMessage( $"ItemPicReorder: Move zePictures out of the way" );
					foreach( var picture in zePictures )
						picture.Number = 999999;
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicReorder: Shift otherPictures" );
					if( number > newNumber )
					{
						var otherPictures = allPictures	.Where( v=>v.Number < number )
														.Where( v=>v.Number >= newNumber )
														.OrderByDescending( v=>v.Number )
														.ToArray();
						foreach( var picture in otherPictures )
						{
							picture.Number = picture.Number + 1;
							await dc.SaveChangesAsync();
						}

						if( (mainImageNumber < number) && (mainImageNumber >= newNumber) )
							item.MainImageNumber = mainImageNumber + 1;
					}
					else // number < newNumber
					{
						var otherPictures = allPictures	.Where( v=>v.Number > number )
														.Where( v=>v.Number <= newNumber )
														.OrderBy( v=>v.Number )
														.ToArray();
						foreach( var picture in otherPictures )
						{
							picture.Number = picture.Number - 1;
							await dc.SaveChangesAsync();
						}

						if( (mainImageNumber > number) && (mainImageNumber <= newNumber) )
							item.MainImageNumber = mainImageNumber - 1;
					}

					if( mainImageNumber == number )
						item.MainImageNumber = newNumber;

					logHelper.AddLogMessage( $"ItemPicReorder: Set the new number" );
					foreach( var picture in zePictures )
						picture.Number = newNumber;
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicReorder: Commit transaction" );
					transaction.Commit();
				}

			EXIT:
				logHelper.AddLogMessage( $"ItemPicReorder: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.ItemPictureSetMain )]
		[HttpPost( Routes.ItemPictureSetMain )]
		public async Task<Utils.TTTServiceResult> SetMain(string itemCode, int number, bool isMain)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"ItemPicSetMain: START {nameof(itemCode)}:'{itemCode}' ; {nameof(number)}:'{number}' ; {nameof(isMain)}:'{isMain}'" );

				if( string.IsNullOrWhiteSpace(itemCode) )
					throw new ArgumentNullException( $"{nameof(itemCode)}" );
				if( number <= 0 )
					throw new ArgumentNullException( $"{nameof(number)}" );

				logHelper.AddLogMessage( $"ItemPicSetMain: Open transaction" );
				var dc = DataContext;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					logHelper.AddLogMessage( $"ItemPicSetMain: Get item" );
					var item = await dc.Items.Where( v=>v.Code == itemCode ).SingleAsync();
					var mainImageNumber = item.MainImageNumber;

					logHelper.AddLogMessage( $"ItemPicSetMain: Update value" );
					item.MainImageNumber = isMain ? (int?)number : null;
					await dc.SaveChangesAsync();

					logHelper.AddLogMessage( $"ItemPicSetMain: Commit transaction" );
					transaction.Commit();
				}

				logHelper.AddLogMessage( $"ItemPicSetMain: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
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
