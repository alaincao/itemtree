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
