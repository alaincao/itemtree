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
		public const string		LoginPassword		= "/admin/login";
		public const string		LoginOAuth			= "/admin/login/oauth/{scheme}";
		public const string		Logout				= "/admin/logout";
		internal const string	Redirections		= "/admin/redirections";
		internal const string	TreeBrowse			= "/admin/tree";

		// APIs
		internal const string	LoginAPI			= "/api/login";
		internal const string	LogoutAPI			= "/api/logout";
		internal const string	ChangePassword		= "/api/change-password";
		internal const string	TreeSanitizePath	= "/tree/sanitizepath";
		internal const string	TreeSanitizeName	= "/tree/sanitizename";
		internal const string	TreeDownload		= "/tree/download";
		internal const string	TreeTempUpload		= "/tree/tempupload";
		internal const string	TreeOperations		= "/tree/operations";

		// Internals ; i.e. not requested directly but used as routing endpoints
		internal const string	Error				= "/error";
		internal const string	ErrorStatus			= "/error/{status}";
		internal const string	TreeView			= "/tree/view";
		internal const string	TreeHtml			= "/tree/html";
		internal const string	TreeHtmlTranslated	= "/tree/html/translated";
		internal const string	TreeFile			= "/tree/file";
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

					API = new {
							Login				= tr( LoginAPI ),
							Logout				= tr( LogoutAPI ),
							ChangePassword		= tr( ChangePassword ),
							Redirections		= tr( Redirections ),
							Tree = new {
									SanitizePath= tr( TreeSanitizePath ),
									SanitizeName= tr( TreeSanitizeName ),
									Download	= tr( TreeDownload ),
									TempUpload	= tr( TreeTempUpload ),
									Operations	= tr( TreeOperations ),
									File		= tr( TreeFile ),
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
