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
		internal const string	ItemEdit			= "/car-edit/{itemCode}";
		internal const string	ItemAdd				= "/car-add";

		// APIs
		internal const string	GetUrlCode			= "/api/geturlcode";
		internal const string	LoginAPI			= "/api/login";
		internal const string	LogoutAPI			= "/api/logout";
		internal const string	ChangePassword		= "/api/change-password";
		internal const string	ItemsListApi		= "/api/car-listing";
		internal const string	ItemDetailsApi		= "/api/car-details/{itemCode}";
		internal const string	ItemSave			= "/api/car-save";
		internal const string	ItemDelete			= "/api/car-delete/{itemCode}";
		internal const string	ItemPictureList		= "/api/car-details/{itemCode}/pictures";
		internal const string	ItemPictureDownload	= "/car-details/{itemCode}/pictures/{number}";

		/// <summary>Set PageParameters routes for client-side</summary>
		internal static object GetPageParameterRoutes(PageHelper pageHelper)
		{
			Utils.Assert( pageHelper != null, typeof(Routes), "Missing parameter 'pageHelper'" );

			if( GetPageParameterRoutesCache != null )
				// Already cached
				return GetPageParameterRoutesCache;

			var langParm = "{lang}";
			Func<string,string> tr = (route)=>
				{
					route = route.Replace( Lang, langParm );  // Cleanup a little bit URLs before sending to clients ...
					pageHelper.ResolveRoute( route );
					return route;
				};

			var obj = new {
					LanguageParameter = langParm,
					ItemCodeParameter = "{itemCode}",

					ItemTTT = new {
							ItemsList	= tr( ItemsList ),
							ItemEdit	= tr( ItemEdit ),
						},
					API = new {
							GetUrlCode			= tr( GetUrlCode ),
							Login				= tr( LoginAPI ),
							Logout				= tr( LogoutAPI ),
							ChangePassword		= tr( ChangePassword ),
							ItemsListing		= tr( ItemsListApi ),
							ItemDetails			= tr( ItemDetailsApi ),
							ItemSave			= tr( ItemSave ),
							ItemDelete			= tr( ItemDelete ),
							ItemDetailsPictures	= tr( ItemPictureList ),
						},
				};
			GetPageParameterRoutesCache = obj;
			return obj;
		}
		private static object GetPageParameterRoutesCache = null;

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
