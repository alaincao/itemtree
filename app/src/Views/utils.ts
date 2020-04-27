
// nb: This module cannot import any other module or there is a risk of circular references in the TypeScript/Browserify compilation leading to JavaScript initializations performed in incorrect order => crash at pages openning

export function stringIsNullOrEmpty(str:string) : boolean
{
	if( str == null )
		return true;
	if( str == '' )
		return true;
	return false;
}
export function stringIsNullOrWhitespace(str:string) : boolean
{
	if( str == null )
		return true;
	if( str.trim() == '' )
		return true;
	return false;
}
export function stringStartsWith(str:string, match:string) : boolean
{
	if( str == null )
		return false;
	return ( str.indexOf(match) === 0 );
}

export function arrayContains<T>(a:T[], item:any) : boolean
{
	return ( a.indexOf( item ) >= 0 );
}

//ES5 incompatible ...
//	export function arrayUnique<T>(a:T[]) : T[]
//	{
//		//return a.filter( (v,i)=>( a.indexOf(v) === i ) );
//		return [ ... new Set(a) ];
//	}

export function arrayRemoveItem<T>(a:T[], item:T) : boolean
{
	let i = a.indexOf( item );
	if( i < 0 )
		return false;
	a.splice( i, 1 );
	return true;
}

export function arraySum<T>(a:T[], f:(e:T)=>number) : number
{
	let rc = 0;
	a.forEach( function(e)
		{
			rc += f(e);
		} );
	return rc;
}

export function arrayMoveItem<T>(p:{ list:T[], item:T, direction?:'up'|'down' }) : T[]
{
	// NB: Does not yet work when n>1 ...

	let n = 1;
	if( p.direction != null )
		n = (p.direction == 'up') ? -n : n;
	const i = p.list.indexOf( p.item );  // Start position
	if( i < 0 )
		throw { error:'Could not find item in list', item:p.item };
	let j = i + n;  // End position
	if( j < 0 )
		j = 0;
	else if( j >= p.list.length )
		j = p.list.length - 1;
	if( i == j )
		// NOOP
		return p.list;

	// Swap items here
	const tmp = p.list[ i ];
	p.list[ i ] = p.list[ j ];
	p.list[ j ] = tmp;

	return p.list;
}

/** Generates a GUID-like string (something that looks like one, but NOT a real one!!!)
 * cf. http://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid-in-javascript */
export function newGuid() : string
{
	var S4 = function() : string
	{
		return (((1+Math.random())*0x10000)|0).toString(16).substring(1);
	};
	return (S4()+S4()+"-"+S4()+"-"+S4()+"-"+S4()+"-"+S4()+S4()+S4());
}

export function htmlEncode(txt:string|string[]) : string
{
	// TODO: Find a better way ?
	if( Array.isArray(txt) )
		return $.map( <string[]>txt, v=>$(document.createElement('span')).text( v ).html() ).join( '<br/>' );
	else
		return $(document.createElement('span')).text( txt ).html();
}

export function sleep(ms:number) : Promise<void>
{
	return new Promise( resolve => setTimeout(resolve, ms) );
}

/** cf. https://schneidenbach.gitbooks.io/typescript-cookbook/nameof-operator.html */
export const nameof = <T>(name: keyof T) => name;

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
