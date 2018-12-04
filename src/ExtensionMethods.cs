using System;
using System.Collections.Generic;

namespace ItemTTT
{
	public static partial class ExtensionMethods
	{
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
	}
}
