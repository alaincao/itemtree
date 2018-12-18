using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemTTT
{
	using Types	= Services.TranslationController.Types;
	using Dict	= Dictionary<string,DTOs.Translation>;

	public static partial class Utils
	{
		internal static class Translations
		{
			private static readonly Types[]					AllTypes	= Enum.GetValues( typeof(Types) ).Cast<Types>().Where( v=>v != Types.Undefined ).ToArray();
			private volatile static Dictionary<Types,Dict>	Cache		= null;

			internal static void ClearCache()
			{
				Cache = null;
			}

			private static Dictionary<Types,Dict> GetCache(Models.ItemTTTContext dc)
			{
				var cache = Cache;
				if( cache != null )
					return cache;

				cache = dc.Translations
							.AsEnumerable()
							.GroupBy( v=>v.Type )
							.ToDictionary( v=>v.Key, v=>
								{
									return v.ToDictionary( w=>w.TranslationEN, w=>
												{
													return new DTOs.Translation{ EN=w.TranslationEN, FR=w.TranslationFR, NL=w.TranslationNL };
												} );
								} );

				// Create empty dicts for types with no values
				foreach( var type in AllTypes )
				{
					var dict = cache.TryGet( type );
					if( dict == null )
						cache[ type ] = new Dict();
				}

				return (Cache = cache);
			}

			internal static Dict Get(Models.ItemTTTContext dc, Types type)
			{
				var cache = GetCache( dc );
				return cache[ type ];
			}

			internal static DTOs.Translation? TryGet(Models.ItemTTTContext dc, Types type, string en)
			{
				var dict = Get( dc, type );
				if( dict.TryGetValue(en, out var tr) )
					return tr;
				return null;
			}

			internal static DTOs.Translation Get(Models.ItemTTTContext dc, Types type, string en)
			{
				var tr = TryGet( dc, type, en );
				if( tr != null )
					return tr.Value;
				return new DTOs.Translation{ EN=en, FR=en, NL=en };
			}
		}
	}
}
