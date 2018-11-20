using System.Collections.Generic;

namespace ItemTTT
{
	public static partial class ExtensionMethods
	{
		public static string JSONStringify(this object obj, bool indented=false)
		{
			Utils.Assert( obj != null, System.Reflection.MethodInfo.GetCurrentMethod(), "Missing parameter 'obj'" );

			var settings = new Newtonsoft.Json.JsonSerializerSettings();
			settings.Formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;  // Indent or not
			settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();  // Lower-case the first letter of the property names
			return Newtonsoft.Json.JsonConvert.SerializeObject( obj, settings );
		}

		public static IDictionary<string,object> JSONDeserialize(this string json)
		{
			if( string.IsNullOrWhiteSpace(json) )
				return null;
			return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>( json );
		}

		public static T JSONDeserialize<T>(this string json)
		{
			if( string.IsNullOrWhiteSpace(json) )
				return default(T);
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>( json );
		}
	}
}
