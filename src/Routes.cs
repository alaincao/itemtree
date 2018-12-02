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
		private const string Lang = Language.RouteParameter;  // i.e. "{lang:lang}" => e.g. "en" or "fr"

		// Views
		internal const string	InitializationLog	= "/initialization-log";
		internal const string	Home1				= "/";
		internal const string	Home2				= "/"+Lang;
		internal const string	Error				= "/error";
		internal const string	ErrorStatus			= "/error/{status}";
		internal const string	AdminHome			= Home1;  // TODO
		internal const string	Login				= "/admin/login";
		internal const string	Logout				= "/admin/logout";
		internal const string	ItemsList			= "/"+Lang+"/car-listing";
		internal const string	ItemDetails			= "/"+Lang+"/car-details/{itemCode}";

		// APIs
		internal const string	LoginAPI			= "/api/login";
		internal const string	LogoutAPI			= "/api/logout";
		internal const string	ChangePassword		= "/api/change-password";
		internal const string	ItemsListApi		= "/api/car-listing";
		internal const string	ItemDetailsApi		= "/api/car-details/{itemCode}";
		internal const string	ItemPictureList		= "/api/car-details/{itemCode}/pictures";
		internal const string	ItemPictureDownload	= "/car-details/{itemCode}/pictures/{number}";

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
