
using System;
using System.Collections.Generic;
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
				var dtos = await query	.OrderByDescending( v=>v.Date )
										.ThenByDescending( v=>v.ID )
										.AsAsyncEnumerable()
										.SelectAsync( v=>
											{
												var dto = new DTOs.Testimonial( v );
												if( dto.ImageData == null )
													dto.ImageData = imgNotFound;
												return dto;
											} )
										.ToArrayAsync();

				logHelper.AddLogMessage( $"TestimonialList: END" );
				return new Utils.TTTServiceResult<DTOs.Testimonial[]>( PageHelper ){ Result=dtos };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.Testimonial[]>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.TestimSaveApi )]
		public async Task<Utils.TTTServiceResult> Save([FromBody]SaveRequest testimonial)
		{
			try
			{
				if( testimonial == null )
					throw new ArgumentException( $"Missing parameter '{nameof(testimonial)}" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"TestimonialSave: START: {nameof(testimonial.ID)}:'{testimonial.ID}'" );

				var isAdding = (testimonial.ID == null);
				if(! isAdding )
					if(! PageHelper.IsAuthenticated )  // nb: Only admins can edit an entry
						throw new Utils.TTTException( "Not logged-in" );

				var dc = DataContext;

				Models.Testimonial model;
				if( isAdding )
				{
					logHelper.AddLogMessage( $"TestimonialSave: Adding a new one" );
					model = new Models.Testimonial{ Date=DateTime.Now, Active=false };
					dc.Testimonials.Add( model );
				}
				else
				{
					logHelper.AddLogMessage( $"TestimonialSave: Retreive testimonial from database" );
					model = await dc.Testimonials.Where( v=>v.ID == testimonial.ID ).SingleAsync();
				}

				logHelper.AddLogMessage( $"TestimonialSave: Copy fields" );
				testimonial.ToModel( model, includeImage:testimonial.SaveImage, includeAdminFields:PageHelper.IsAuthenticated );

				logHelper.AddLogMessage( $"TestimonialSave: Save changes to database" );
				await dc.SaveChangesAsync();

				logHelper.AddLogMessage( $"TestimonialSave: END" );
				return new Utils.TTTServiceResult<int>( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}
		public class SaveRequest : DTOs.Testimonial
		{
			public bool	SaveImage	{ get; set; }
		}

		[HttpPost( Routes.TestimDeleteApi )]
		public async Task<Utils.TTTServiceResult> Delete(int id)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"TestimonialDelete: START: {nameof(id)}:'{id}'" );

				if( id <= 0 )
					throw new ArgumentException( $"Missing parameter '{nameof(id)}'" );

				var testimonial = new Models.Testimonial{ ID=id };
				var dc = DataContext;
				dc.Testimonials.Attach( testimonial );
				dc.Testimonials.Remove( testimonial );

				logHelper.AddLogMessage( $"TestimonialDelete: Save to database" );
				await dc.SaveChangesAsync();

				logHelper.AddLogMessage( $"TestimonialDelete: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
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
				var newImage = Utils.Images.CropResize( bitmap, destinationHeight:(double)DestinationHeight, scaleHToW:ScaleHToW, scaleWToH:ScaleWToH );
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
