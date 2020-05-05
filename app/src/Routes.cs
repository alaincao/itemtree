using System;

namespace ItemTTT
{
	public static class Routes
	{
		internal const string	LangParameter		= Language.RouteParameter;  // i.e. "{lang:lang}" => e.g. "en" or "fr"
		private const string	ItemCodeParameter	= "{itemCode}";
		internal const string	ItemIDParameter		= "{id}";
		private const string	LangClientSide		= "{lang}";  // nb: only because it is cleaner ...
		private const string	ItemCodeClientSide	= "{code}";  // nb: only because it is cleaner ...
		private const string	ItemIDClientSide	= ItemIDParameter;

		// Views
		internal const string	InitializationLog	= "/initialization-log";
		public const string		Home1				= "/";
		public const string		Home2				= "/"+LangParameter;
		public const string		AdminHome			= "/admin";
		public const string		Login				= "/admin/login";
		public const string		Logout				= "/admin/logout";
		public const string		ItemsList			= "/"+LangParameter+"/item-listing";
		internal const string	ItemDetails			= "/"+LangParameter+"/item-details/"+ItemCodeParameter;
		internal const string	ItemEdit			= "/item-edit/"+ItemCodeParameter;
		public const string		ItemAdd				= "/item-add";
		public const string		BlogList			= "/"+LangParameter+"/blog";
		internal const string	BlogDetails			= "/"+LangParameter+"/blog/"+ItemIDParameter;
		public const string		BlogAdd				= "/blog/add";
		internal const string	BlogEdit			= "/blog/edit/"+ItemIDParameter;
		public const string		TestimonialList		= "/"+LangParameter+"/testimonial";
		public const string		DynamicPageShow		= "/"+LangParameter+"/dynamicpage/"+ItemCodeParameter;
		public const string		DynamicPageStatic	= "/static/demo";

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
		internal const string	DynamicPageGetApi	= "/api/dynamicpage/get";
		internal const string	DynamicPageUpdateApi= "/api/dynamicpage/update";
		internal const string	TreeDownload		= "/tree/download";

		// Internals ; i.e. not requested directly but used as routing endpoints
		internal const string	Error				= "/error";
		internal const string	ErrorStatus			= "/error/{status}";
		internal const string	TreeOperations		= "/tree/operations";
		internal const string	TreeView			= "/tree/view";
		internal const string	TreeHtml			= "/tree/html";
		internal const string	TreeHtmlTranslated	= "/tree/html/translated";
		internal const string	TreeImage			= "/tree/image";

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
							DynamicPage = new {
									Get		= tr( DynamicPageGetApi ),
									Update	= tr( DynamicPageUpdateApi ),
								},
							Tree = new {
									Operations	= tr( TreeOperations ),
									Image		= tr( TreeImage ),
								},
						},
				};
			GetPageParameterRoutesCache = obj;
			return obj;
		}
		private static object GetPageParameterRoutesCache = null;
	}
}
