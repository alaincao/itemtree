using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemTTT
{
	public static partial class ExtensionMethods
	{
		// Those are not(i.e. removed from v2.2) in standard libs!
		// cf. https://github.com/dotnet/efcore/issues/18124   Major design headache! ;-)
		public static async IAsyncEnumerable<U> SelectAsync<T,U>(this IAsyncEnumerable<T> list, Func<T,U> selector)
		{
			await foreach( var item in list )
				yield return selector( item );
		}
		public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> list)
		{
			var l = new List<T>();
			await foreach( var item in list )
				l.Add( item );
			return l;
		}
		public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> list)
		{
			return (await list.ToListAsync()).ToArray();
		}

		internal static string JSONStringify(this object obj, bool indented=false)
		{
			Utils.Assert( obj != null, System.Reflection.MethodInfo.GetCurrentMethod(), $"Missing parameter {nameof(obj)}" );

			var settings = new Newtonsoft.Json.JsonSerializerSettings();
			settings.Formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;  // Indent or not
			settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();  // Lower-case the first letter of the property names
			return Newtonsoft.Json.JsonConvert.SerializeObject( obj, settings );
		}

		internal static IDictionary<string,object> JSONDeserialize(this string json)
		{
			if( string.IsNullOrWhiteSpace(json) )
				return null;
			return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>( json );
		}

		internal static T JSONDeserialize<T>(this string json)
		{
			if( string.IsNullOrWhiteSpace(json) )
				return default(T);
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>( json );
		}

		internal static V TryGet<K,V>(this IDictionary<K,V> dict, K key)
		{
			V value;
			if( dict.TryGetValue(key, out value) )
				return value;
			return default( V );
		}

		internal static System.Nullable<T> TryParseEnum<T>(this string str) where T : struct, IConvertible
		{
			if( string.IsNullOrWhiteSpace(str) )
				return null;

			T rv;
			if(! Enum.TryParse<T>(str, out rv) )
				return null;
			return rv;
		}

		internal static string ToIsoDate(this DateTime? dt)
		{
			if( dt == null )
				return null;
			return ToIsoDate( dt.Value );
		}
		internal static string ToIsoDate(this DateTime dt)
		{
			return dt.ToString( "yyyy-MM-dd" );
		}

		internal static DateTime? FromIsoDate(this string isoDate)
		{
			if( string.IsNullOrWhiteSpace(isoDate) )
				return null;
			return DateTime.Parse( isoDate );  // nb: the .Net framework should able to parse
		}
	}
}
