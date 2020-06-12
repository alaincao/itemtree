
using System;
using System.Threading.Tasks;

namespace ItemTTT
{
	partial class Utils
	{
		internal static class Files
		{
			/// <returns>
			/// A: the base64 string ;
			/// B: The MemoryStream of the content
			/// </returns>
			internal static async Task<CustomClass2<string,System.IO.MemoryStream>> GetBase64String(LogHelper logHelper, Microsoft.AspNetCore.Http.IFormFile file)
			{
				Utils.Assert( logHelper != null, nameof(GetBase64String), $"Missing parameter '{nameof(logHelper)}'" );
				Utils.Assert( file != null, nameof(GetBase64String), $"Missing parameter '{nameof(file)}'" );

				logHelper.AddLogMessage( $"{nameof(GetBase64String)}: Create MemoryStream" );
				var ms = new System.IO.MemoryStream();
				await file.CopyToAsync( ms );

				logHelper.AddLogMessage( $"{nameof(GetBase64String)}: Create base64 string" );
				var base64 = Convert.ToBase64String( ms.ToArray() );

				ms.Position = 0;
				return new CustomClass2<string, System.IO.MemoryStream>{ A=base64, B=ms };
			}

			public static string GetContentType(string fileName)
			{
				Utils.Assert( !string.IsNullOrWhiteSpace(fileName), nameof(GetContentType), $"Missing parameter '{nameof(fileName)}'" );

				var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
				if( provider.TryGetContentType(fileName, out var contentType) )
					return contentType;
				return "application/octet-stream";
			}
		}
	}
}
