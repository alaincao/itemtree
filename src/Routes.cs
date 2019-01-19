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
		internal const string	LangParameter		= Language.RouteParameter;  // i.e. "{lang:lang}" => e.g. "en" or "fr"
		private const string	ItemCodeParameter	= "{itemCode}";
		internal const string	ItemIDParameter		= "{id}";
		private const string	LangClientSide		= "{lang}";  // nb: only because it is cleaner ...
		private const string	ItemCodeClientSide	= "{code}";  // nb: only because it is cleaner ...
		private const string	ItemIDClientSide	= ItemIDParameter;

		// Views
		internal const string	InitializationLog	= "/initialization-log";
		internal const string	Home1				= "/";
		internal const string	Home2				= "/"+LangParameter;
		internal const string	Error				= "/error";
		internal const string	ErrorStatus			= "/error/{status}";
		internal const string	AdminHome			= "/admin";
		internal const string	Login				= "/admin/login";
		internal const string	Logout				= "/admin/logout";
		internal const string	ItemsList			= "/"+LangParameter+"/item-listing";
		internal const string	ItemDetails			= "/"+LangParameter+"/item-details/"+ItemCodeParameter;
		internal const string	ItemEdit			= "/item-edit/"+ItemCodeParameter;
		internal const string	ItemAdd				= "/item-add";
		internal const string	BlogList			= "/"+LangParameter+"/blog";
		internal const string	BlogDetails			= "/"+LangParameter+"/blog/"+ItemIDParameter;
		internal const string	BlogAdd				= "/blog/add";
		internal const string	BlogEdit			= "/blog/edit/"+ItemIDParameter;
		internal const string	TestimonialList		= "/"+LangParameter+"/testimonial";

		// APIs
		internal const string	GetUrlCode			= "/api/geturlcode";
		internal const string	LoginAPI			= "/api/login";
		internal const string	LogoutAPI			= "/api/logout";
		internal const string	ChangePassword		= "/api/change-password";
		internal const string	AutoComplete		= "/api/autocomplete";
		internal const string	GetTranslations		= "/api/translations";
		internal const string	SaveTranslations	= "/api/translations/save";
		internal const string	ItemsListApi		= "/api/item-listing";
		internal const string	ItemDetailsApi		= "/api/item-details/"+ItemCodeParameter;
		internal const string	ItemSave			= "/api/item-save";
		internal const string	ItemDelete			= "/api/item-delete/"+ItemCodeParameter;
		internal const string	ItemPictureList		= "/api/item-details/"+ItemCodeParameter+"/pictures";
		internal const string	ItemPictureDelete	= "/api/item-details/"+ItemCodeParameter+"/pictures/delete";
		internal const string	ItemPictureReorder	= "/api/item-details/"+ItemCodeParameter+"/pictures/reorder";
		internal const string	ItemPictureSetMain	= "/api/item-details/"+ItemCodeParameter+"/pictures/setmain";
		internal const string	ItemPictureDownload	= "/item-details/"+ItemCodeParameter+"/pictures/{number}";
		internal const string	ItemPictureUpload	= "/api/item-details/"+ItemCodeParameter+"/pictures/upload";
		internal const string	BlogListApi			= "/api/blog";
		internal const string	BlogSaveApi			= "/api/blog/save";
		internal const string	BlogDeleteApi		= "/api/blog/delete";
		internal const string	BlogPictureUpload	= "/api/blog/picture/upload";
		internal const string	TestimListApi		= "/api/testimonial";
		internal const string	TestimPictUpload	= "/api/testimonial/picture/upload";
		internal const string	TestimSaveApi		= "/api/testimonial/save";
		internal const string	TestimDeleteApi		= "/api/testimonial/delete";

		/// <summary>Set PageParameters routes for client-side</summary>
		internal static object GetPageParameterRoutes(PageHelper pageHelper)
		{
			Utils.Assert( pageHelper != null, typeof(Routes), "Missing parameter 'pageHelper'" );

			if( GetPageParameterRoutesCache != null )
				// Already cached
				return GetPageParameterRoutesCache;

			Func<string,string> tr = (route)=>
				{
					route = route.Replace( LangParameter, LangClientSide ).Replace( ItemCodeParameter, ItemCodeClientSide );  // Cleanup a little bit URLs before sending to clients ...
					pageHelper.ResolveRoute( route );
					return route;
				};

			var obj = new {
					LanguageParameter	= LangClientSide,
					ItemCodeParameter	= ItemCodeClientSide,
					ItemIDParameter		= ItemIDClientSide,

					ItemTTT = new {
							List	= tr( ItemsList ),
							Edit	= tr( ItemEdit ),
						},
					Blog = new {
							List	= tr( BlogList ),
							Edit	= tr( BlogEdit ),
						},
					API = new {
							GetUrlCode			= tr( GetUrlCode ),
							Login				= tr( LoginAPI ),
							Logout				= tr( LogoutAPI ),
							ChangePassword		= tr( ChangePassword ),
							AutoComplete		= tr( AutoComplete ),
							Translations = new {
									List	= tr( GetTranslations ),
									Save	= tr( SaveTranslations ),
								},
							Items = new {
									List			= tr( ItemsListApi ),
									Details			= tr( ItemDetailsApi ),
									Save			= tr( ItemSave ),
									Delete			= tr( ItemDelete ),
									DetailsPictures	= tr( ItemPictureList ),
									Pictures = new {
											Delete	= tr( ItemPictureDelete ),
											Reorder	= tr( ItemPictureReorder ),
											SetMain	= tr( ItemPictureSetMain ),
											Upload	= tr( ItemPictureUpload ),
										},
								},
							Blog = new {
									List			= tr( BlogListApi ),
									Save			= tr( BlogSaveApi ),
									Delete			= tr( BlogDeleteApi ),
									PictureUpload	= tr( BlogPictureUpload ),
								},
							Testimonial = new {
									List			= tr( TestimListApi ),
									Save			= tr( TestimSaveApi ),
									Delete			= tr( TestimDeleteApi ),
									PictureUpload	= tr( TestimPictUpload ),
								},
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
