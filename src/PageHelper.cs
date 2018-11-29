using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ItemTTT
{
	// NB: Scoped service
	public class PageHelper
	{
		public class PageParameters : Dictionary<string,object>
		{
			internal PageParameters(bool isAutenticated, Languages currentLanguage, Dictionary<Languages,string> languageUrls)
			{
				PageTitle		= "";
				IsAutenticated	= isAutenticated;
				CurrentLanguage	= ""+currentLanguage;
				LanguageUrls	= languageUrls;
			}
			public string						PageTitle		{ get { return (string)this["PageTitle"]; } set { this["PageTitle"] = value; } }
			private bool						IsAutenticated	{ set { this["IsAutenticated"] = value; } }
			private string						CurrentLanguage	{ set { this["CurrentLanguage"] = value; } }
			public Dictionary<Languages,string>	LanguageUrls	{ get { return (Dictionary<Languages,string>)this["LanguageUrls"]; } private set { this["LanguageUrls"] = value; } }
		}

		internal LogHelper				ScopeLogs			{ get; private set; } = new LogHelper();
		public readonly PageParameters	Parameters;
		public bool						UseMinified			{ get { return Utils.IsDebug == false; } }
		private const Languages			DefaultLanguage		= Languages.nl;
		public readonly Languages		CurrentLanguage;
		public readonly bool			IsAutenticated;

		private Func<string,string>	UrlHelperContent;

		public PageHelper(IActionContextAccessor contextAccessor)
		{
			ScopeLogs.AddLogMessage( "PageHelper: Constructor" );

			var actionContext = contextAccessor.ActionContext;
			var httpContext = actionContext.HttpContext;

			// Create 'UrlHelperContent' callback
			var urlHelper = new UrlHelper( actionContext );
			UrlHelperContent = (route)=>
				{
					return urlHelper.Content( route );
				};

			IsAutenticated	= httpContext.User.Identity.IsAuthenticated;
			CurrentLanguage	= InitLangage( httpContext, out var languageURLs );
			Parameters		= new PageParameters( IsAutenticated, CurrentLanguage, languageURLs );
		}

		internal string ResolveRoute(string route)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), GetType(), "Missing parameter 'route'" );
			if( route[0] == '/' )
				route = '~'+route;
			return UrlHelperContent( route );
		}

		private Languages InitLangage(HttpContext httpContext, out Dictionary<Languages,string> languageUrls)
		{
			ScopeLogs.AddLogMessage( "PageHelper: InitLangage START" );
			Languages? fromUrl, fromCookie;
			Languages current;
			string rawPath;
			DetectLanguage( ScopeLogs, httpContext.Request, fromUrl:out fromUrl, fromCookie:out fromCookie, current:out current, rawPath:out rawPath );

			if( fromUrl != null )
			{
				// Language has been forced by the URL
				if( fromUrl.Value != fromCookie )
				{
					// The cookie does not reflect it => Set its new value
					ScopeLogs.AddLogMessage( $"PageHelper: InitLangage: Set language cookie to '{fromUrl.Value}'" );
					Language.SetLanguageCookie( httpContext.Response, fromUrl.Value );
				}
			}

			languageUrls = Enum.GetValues( typeof(Languages) ).Cast<Languages>()
									.Select( v=>new{	lng	= v,
														url	= rawPath.Replace(Language.RouteParameter, ""+v) } )
									.ToDictionary( v=>v.lng, v=>v.url );
			ScopeLogs.AddLogMessage( "PageHelper: InitLangage END" );
			return current;
		}

		internal static void DetectLanguage(LogHelper logHelper, HttpRequest request, out Languages? fromUrl, out Languages? fromCookie, out Languages current, out string rawPath)
		{
			logHelper.AddLogMessage( "PageHelper: DetectLanguage: Split URL segments" );
			var segments = (""+request.Path).Split( "/" ).Where(v=>v != "").ToList();
			if( segments.Count == 0 )
			{
				// Homepage ?
				segments.Add( "" );
			}

			logHelper.AddLogMessage( "PageHelper: DetectLanguage: Get from URL" );
			{
				object obj;
				if( Enum.TryParse(typeof(Languages), segments[0], ignoreCase:true, result:out obj) )
				{
					// Language found in the first segment of the URL
					segments.RemoveAt( 0 );
					fromUrl = (Languages)obj;
				}
				else
				{
					// Not found in URL
					fromUrl = null;
				}
			}

			logHelper.AddLogMessage( "PageHelper: DetectLanguage: Get from cookie" );
			fromCookie = Language.GetLanguageCookie( request );

			// Determine 'current'
			if( fromUrl != null )
				current = fromUrl.Value;
			else if( fromCookie != null )
				current = fromCookie.Value;
			else
				current = DefaultLanguage;
			logHelper.AddLogMessage( $"PageHelper: DetectLanguage: fromUrl:'{fromUrl}' ; fromCookie:'{fromCookie}' ; current:'{current}'" );

			var segmentsStr = string.Join( "/", segments );
			rawPath = $"/{Language.RouteParameter}/{segmentsStr}{request.QueryString}";
		}
	}
}
