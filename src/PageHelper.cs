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
			internal PageParameters()
			{
				PageTitle = "";
			}
			public string						PageTitle		{ get { return (string)this["PageTitle"]; } set { this["PageTitle"] = value; } }
			public Dictionary<Languages,string>	LanguageUrls	{ get { return (Dictionary<Languages,string>)this["LanguageUrls"]; } set { this["LanguageUrls"] = value; } }
		}

		internal LogHelper			ScopeLogs			{ get; private set; } = new LogHelper();
		public PageParameters		Parameters			{ get; private set; } = new PageParameters();
		public bool					UseMinified			{ get { return Utils.IsDebug == false; } }
		private const Languages		DefaultLanguage		= Languages.nl;
		public Languages			CurrentLanguage		{ get; private set; }

		private Func<string,string>	UrlHelperContent;

		public PageHelper(IActionContextAccessor contextAccessor)
		{
			ScopeLogs.AddLogMessage( "PageHelper: Constructor" );

			var actionContext = contextAccessor.ActionContext;

			// Create 'UrlHelperContent' callback
			var urlHelper = new UrlHelper( actionContext );
			UrlHelperContent = (route)=>
				{
					return urlHelper.Content( route );
				};

			InitLangage( actionContext.HttpContext );
		}

		internal string ResolveRoute(string route)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), GetType(), "Missing parameter 'route'" );
			if( route[0] == '/' )
				route = '~'+route;
			return UrlHelperContent( route );
		}

		private void InitLangage(HttpContext httpContext)
		{
			ScopeLogs.AddLogMessage( "PageHelper: InitLangage" );
			Languages? dummy;
			Languages current;
			string rawPath;
			DetectLanguage( httpContext, fromUrl:out dummy, current:out current, rawPath:out rawPath );

			CurrentLanguage = current;
			Parameters.LanguageUrls = Enum.GetValues( typeof(Languages) ).Cast<Languages>()
												.Select( v=>new{	lng	= v,
																	url	= rawPath.Replace(Language.RouteParameter, ""+v) } )
												.ToDictionary( v=>v.lng, v=>v.url );
		}

		internal static void DetectLanguage(HttpContext httpContext, out Languages? fromUrl, out Languages current, out string rawPath)
		{
			var request = httpContext.Request;

			// Split URL segments
			var segments = (""+request.Path).Split( "/" ).Where(v=>v != "").ToList();
			if( segments.Count == 0 )
			{
				// Homepage ?
				segments.Add( "" );
			}

			{
				object obj;
				if( Enum.TryParse(typeof(Languages), segments[0], ignoreCase:true, result:out obj) )
				{
					// Language found in the first segment of the URL

					segments.RemoveAt( 0 );
					fromUrl = (Languages)obj;
					current = fromUrl.Value;
					goto EXIT;
				}
			}
			// Not found in URL
			fromUrl = null;

			// TODO: Search in cookies then in browser's config and only then use the fallback
			current = DefaultLanguage;

		EXIT:
			var segmentsStr = string.Join( "/", segments );
			rawPath = $"/{Language.RouteParameter}/{segmentsStr}{request.QueryString}";
		}
	}
}
