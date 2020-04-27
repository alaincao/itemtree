
// nb: This module cannot import any other module or there is a risk of circular references in the TypeScript/Browserify compilation leading to JavaScript initializations performed in incorrect order => crash at pages openning


/** Function to simulate string-valued enums
 * Based on: https://basarat.gitbooks.io/typescript/docs/types/literal-types.html
 * Returns: { e:the enum , a:the array specified as parameter } */
export function strEnum<T extends string>(a:Array<T>): { e:{[K in T]: K}, a:T[] }
{
	const e = a.reduce( (res, key)=>
		{
			res[key] = key;
			return res;
		}, Object.create(null) );
	return { e, a };
}
