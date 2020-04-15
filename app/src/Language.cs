using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT
{
	public enum Languages
	{
		en, fr, nl
	}

	public static class Language
	{
		internal const Languages	Default			= Languages.en;
		internal const string		ConstraintName	= "lang";
		internal const string		RouteParameter	= "{lang:"+ConstraintName+"}";

		public class Info
		{
			public Languages?	FromUrl;
			public Languages?	FromCookie;
			public Languages	Current;
			public string		CleanPath;
			public string		RawRoutePlusQuery;
		}

		/// <summary>Middleware to setup the scope's 'Language.Info' instance</summary>
		internal static Task LanguageInfoMiddleware(HttpContext context, Func<Task> next)
		{
			var services = context.RequestServices;
			var logHelper = services.GetScopeLog();
			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: START" );
			var info = services.GetRequiredService<Language.Info>();

			if( info.CleanPath != null )
			{
				logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Infos already set" );
				// i.e. The exception handler is replaying the pipeline
				// => The request's path is now set to the exception handler's route
				// => Do not update the info so we keep the original request's paths
				goto NEXT;
			}

			var request = context.Request;
			Languages? fromUrl;
			Languages? fromCookie;
			Languages current;
			string cleanPath;
			string rawRoutePlusQuery;

			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Split URL segments" );
			var segments = request.Path.ToString().Split( "/", StringSplitOptions.RemoveEmptyEntries ).ToList();
			if( segments.Count == 0 )
			{
				// Homepage ?
				segments.Add( "" );
			}
			var endsWithSlash = request.Path.ToString().EndsWith( "/" );

			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Get from URL" );
			{
				if( Enum.TryParse(typeof(Languages), segments[0], ignoreCase:true, result:out var obj) )
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

			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Get from cookie" );
			fromCookie = Utils.GetCookie<Languages>( request );

			// Determine 'current'
			if( fromUrl != null )
				current = fromUrl.Value;
			else if( fromCookie != null )
				current = fromCookie.Value;
			else
				current = Language.Default;

			if( fromUrl != null )
			{
				// Language has been forced by the URL
				if( fromUrl.Value != fromCookie )
				{
					// The cookie does not reflect it => Set its new value
					logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Set language cookie to '{fromUrl.Value}'" );
					Utils.SetCookie<Languages>( context.Response, fromUrl.Value );
				}
			}

			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: Resolve paths" );
			cleanPath = "/"+string.Join( "/", segments );
			rawRoutePlusQuery = $"/{Language.RouteParameter}{cleanPath}{request.QueryString}";
			if( endsWithSlash )
				// Append trailing '/' as it was in the original URL
				rawRoutePlusQuery += "/";

			info.FromUrl			= fromUrl;
			info.FromCookie			= fromCookie;
			info.Current			= current;
			info.CleanPath			= cleanPath;
			info.RawRoutePlusQuery	= rawRoutePlusQuery;
		NEXT:
			logHelper.AddLogMessage( $"{nameof(LanguageInfoMiddleware)}: END fromUrl:'{info.FromUrl}' ; fromCookie:'{info.FromCookie}' ; current:'{info.Current}' ; cleanPath:'{info.CleanPath}' ; rawRoutePlusQuery:'{info.RawRoutePlusQuery}'" );
			return next();
		}

		/// <summary>Last resort middleware: When no route matched, redirect the same URL prefixed with the language</summary>
		internal static async Task LanguageRedirectionMiddleware(HttpContext context, Func<Task> next)
		{
			var services = context.RequestServices;
			var logHelper = services.GetScopeLog();

			logHelper.AddLogMessage( $"{nameof(LanguageRedirectionMiddleware)}: Get language info" );
			var languageInfo = services.GetRequiredService<Language.Info>();

			if( languageInfo.FromUrl == null )
			{
				// The language could not be determined from the URL
				logHelper.AddLogMessage( $"{nameof(LanguageRedirectionMiddleware)}: Redirect to '{languageInfo.Current}' page" );
				context.Response.Redirect( languageInfo.RawRoutePlusQuery.Replace(Language.RouteParameter, ""+languageInfo.Current) );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(LanguageRedirectionMiddleware)}: The request could not be handled" );
				await next();  // i.e. continue to 404
			}
		}
	}

	public class LanguageRouteConstraint : IRouteConstraint
	{
		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			// cf. https://gunnarpeipman.com/aspnet/aspnet-core-simple-localization/

			object lang;
			if(! values.TryGetValue(Language.ConstraintName, out lang) )
				return false;

			if(! values.ContainsKey(Language.ConstraintName) )
				return false;

			return Enum.GetNames( typeof(Languages) ).Contains( ""+lang );
		}
	}
}
