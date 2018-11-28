using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ItemTTT
{
	public enum Languages
	{
		en, fr, nl
	}

	public static class Language
	{
		internal const string	ConstraintName	= "lang";
		internal const string	RouteParameter	= "{lang:"+ConstraintName+"}";
		private const string	LanguageCookie	= "language";

		internal static Languages? GetLanguageCookie(HttpRequest request)
		{
			var value = request.Cookies[ Language.LanguageCookie ];
			if( string.IsNullOrWhiteSpace(value) )
			{
				return null;
			}
			else
			{
				object obj;
				if( Enum.TryParse(typeof(Languages), value, out obj) )
					return (Languages)obj;
				return null;
			}
		}

		internal static void SetLanguageCookie(HttpResponse response, Languages value)
		{
			response.Cookies.Append( Language.LanguageCookie, ""+value );
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
