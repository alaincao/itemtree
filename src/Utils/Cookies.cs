
using System;
using Microsoft.AspNetCore.Http;

namespace ItemTTT
{
	public static partial class Utils
	{
		private static string GetCookie(HttpRequest request, string cookieName)
		{
			if( string.IsNullOrWhiteSpace(cookieName) )
				throw new ArgumentNullException( nameof(cookieName) );
			return request.Cookies[ cookieName ];
		}

		internal static T? GetCookie<T>(HttpRequest request) where T : struct, IConvertible
		{
			var cookieName = GetCookieName( typeof(T) );
			var value = Utils.GetCookie( request, cookieName );
			if( string.IsNullOrWhiteSpace(value) )
			{
				return null;
			}
			else
			{
				object obj;
				if( Enum.TryParse(typeof(T), value, out obj) )
					return (T)obj;
				Utils.Fail( System.Reflection.MethodInfo.GetCurrentMethod(), $"Unknown value '{value}' for cookie '{cookieName}'" );
				return null;
			}
		}

		private static void SetCookie(HttpResponse response, string cookieName, string value)
		{
			if( string.IsNullOrWhiteSpace(cookieName) )
				throw new ArgumentNullException( nameof(cookieName) );
			response.Cookies.Append( cookieName, value );
		}

		internal static void SetCookie<T>(HttpResponse response, T value) where T : struct, IConvertible
		{
			var cookieName = GetCookieName( typeof(T) );
			SetCookie( response, cookieName, ""+value );
		}

		internal static T GetSetCookie<T>(HttpContext httpContext, T? specified, T fallback) where T : struct, IConvertible
		{
			var cookie = Utils.GetCookie<T>( httpContext.Request );
			if( specified != null )
			{
				if(! specified.Equals(cookie) )
					// The cookie does not yet reflect the specified value => Set cookie to remember for next time
					Utils.SetCookie<T>( httpContext.Response, specified.Value );
				return specified.Value;
			}
			else if( cookie != null )
			{
				return cookie.Value;
			}
			else
			{
				return fallback;
			}
		}

		private static string GetCookieName(Type t)
		{
			// nb: The list of all cookies in the application ; This to ensure there is no conflicts
			if( t == typeof(Languages) )
				return "language";
			else if( t == typeof(Views.ItemTTTController.ViewModes) )
				return "listing_view";
			else if( t == typeof(Views.ItemTTTController.SortingFields) )
				return "listing_order";
			else
				throw new ArgumentException( $"Unknown cookie type '{t.FullName}'" );
		}
	}
}
