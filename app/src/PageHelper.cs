using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemTTT
{
	// NB: Scoped service
	public class PageHelper
	{
		public class PageParameters : Dictionary<string,object>
		{
			internal PageParameters(bool isAutenticated, Languages currentLanguage, Dictionary<Languages,string> languageUrls, object routes)
			{
				PageTitle		= "";
				IsAutenticated	= isAutenticated;
				CurrentLanguage	= ""+currentLanguage;
				LanguageUrls	= languageUrls;
				Routes			= routes;
				HasErrors		= false;
				IsDebug			= Utils.IsDebug;
			}
			public string						PageTitle		{ get { return (string)this["PageTitle"]; } set { this["PageTitle"] = value; } }
			private bool						IsAutenticated	{ set { this["IsAutenticated"] = value; } }
			private string						CurrentLanguage	{ set { this["CurrentLanguage"] = value; } }
			public Dictionary<Languages,string>	LanguageUrls	{ get { return (Dictionary<Languages,string>)this["LanguageUrls"]; } private set { this["LanguageUrls"] = value; } }
			private object						Routes			{ set { this["Routes"] = value; } }
			internal bool						HasErrors		{ set { this["HasErrors"] = value; } }
			private bool						IsDebug			{ set { this["IsDebug"] = value; } }
			internal string[]					Logs			{ set { this["Logs"] = value; } }
		}

		internal readonly LogHelper		ScopeLogs;
		private readonly string			WebPathBase;  // e.g.: "" or "/foobar" ; i.e. without the trailing '/'
		public readonly PageParameters	Parameters;
		public readonly Languages		CurrentLanguage;
		public readonly string			UserName;
		public readonly bool			IsAuthenticated;

		public PageHelper(IServiceProvider services, Language.Info languageInfo)
		{
			ScopeLogs = services.GetScopeLog();
			ScopeLogs.AddLogMessage( $"{nameof(PageHelper)}: Constructor" );

			var httpContext = services.GetHttpContext();
			Utils.Assert( httpContext != null, this, $"PageHelper: Unable to get the HttpContext" );

			WebPathBase = httpContext.Request.PathBase;
			ScopeLogs.AddLogMessage( $"{nameof(PageHelper)}: {nameof(WebPathBase)}: '{WebPathBase}'" );

			var languageUrls = Enum.GetValues( typeof(Languages) ).Cast<Languages>()
									.Select( v=>new{	lng	= v,
														url	= languageInfo.RawRoutePlusQuery.Replace(Language.RouteParameter, Language.ToUrlValue[v]) } )
									.ToDictionary( v=>v.lng, v=>ResolveRoute(v.url) );

			IsAuthenticated	= httpContext.User.Identity.IsAuthenticated;
			UserName		= httpContext.User.Identity.Name;
			CurrentLanguage	= languageInfo.Current;
			var routes		= Routes.GetPageParameterRoutes( this );
			Parameters		= new PageParameters( IsAuthenticated, CurrentLanguage, languageUrls, routes );
		}

		internal string ResolveRoute(string route)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), GetType(), $"Missing parameter '{nameof(route)}'" );
			var trimStart = 0;
			if( route[0] == '~' )
				++ trimStart;
			if( route.StartsWith("/") )
				++ trimStart;
			if( trimStart > 0 )
				route = route.Substring( trimStart );
			return $"{WebPathBase}/{route}";
		}
	}
}
