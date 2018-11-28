using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT
{
	internal static class Routes
	{
		internal const string	InitializationLog	= "/initialization-log";
		internal const string	Home				= "/";
		internal const string	Error				= "/error";
		internal const string	ErrorStatus			= "/error/{status}";
		internal const string	ItemDetails			= "/"+Language.RouteParameter+"/car-details/{code}";  // "/{lang:lang}/car-details/{code}"
		internal const string	ItemPictureGet		= "/car-details/{itemCode}/pictures/{number}";

		/// <summary>Last resort middleware: When no route matched, redirect the same URL prefixed with the language</summary>
		internal static IApplicationBuilder UseLanguageRedirect(this IApplicationBuilder app)
		{
			var languages = Enum.GetNames( typeof(Languages) );

			app.Use( async (context,next) =>
				{
					Languages? fromUrl, fromCookie;
					Languages current;
					string rawPath;
					var discardLogs = new LogHelper();
					PageHelper.DetectLanguage( discardLogs, context.Request, fromUrl:out fromUrl, fromCookie:out fromCookie, current:out current, rawPath:out rawPath );

					if( fromUrl == null )
					{
						// The language could not be determined from the URL => Redirect to the localized page
						context.Response.Redirect( rawPath.Replace(Language.RouteParameter, ""+current) );
					}
					else
					{
						// Continue to 404
						await next();
					}
				} );

			return app;
		}
	}
}
