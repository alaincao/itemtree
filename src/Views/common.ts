
import * as common from "./common";
import { PageParameters } from "../PageHelper";
import { Routes } from "../Routes";


export const debugMessages : boolean = false;  // NB: 'export' so that it can be easily changed from the browser's console
export var pageParameters : PageParameters;
export var routes : Routes;

export function init(p:{ pageParameters:PageParameters })
{
	pageParameters = p.pageParameters;
	routes = pageParameters.routes;
	utils.log( 'common.init()' );

	utils.log( 'common.init(): Add custom KnockoutHandler' );

	ko.bindingHandlers.ttt_autocomplete =
		{
			init: (element,valueAccessor)=>
					{
						const searchFunction : (term:string)=>Promise<string[]> = valueAccessor();
						$(element).autocomplete( {	minLength	: 0,
													source		: async (request:{term:string}, resolve:(list:string[])=>void)=>
																	{
																		const list = await searchFunction( request.term );
																		resolve( list );
																	},
												} );
					},
		};

	// cf. https://knockoutjs.com/examples/animatedTransitions.html
	ko.bindingHandlers.ttt_slideUpDownVisible =
		{
			init: (element,valueAccessor)=>
					{
						const value = ko.unwrap( valueAccessor() );
						if(! value )
							$(element).hide();
					},
			update: (element,valueAccessor)=>
					{
						const value = ko.unwrap( valueAccessor() );
						if( value )
							$(element).slideDown();
						else
							$(element).slideUp();
					}
		};
	ko.bindingHandlers.ttt_slideLeftRightVisible =
		{
			init: (element,valueAccessor)=>
					{
						const value = ko.unwrap( valueAccessor() );
						if(! value )
							$(element).hide();
					},
			update: (element,valueAccessor)=>
					{
						const value = ko.unwrap( valueAccessor() );
						if( value )
							$(element).show( 'blind', { direction: 'left' } );
						else
							$(element).hide( 'blind', { direction: 'left' } );
					}
		};
}

export namespace utils
{
	export function log(...optionalParams: any[]) : void
	{
		if( pageParameters.isDebug )
			console.log.apply( console, arguments );
	}

	export function error(...optionalParams: any[]) : void
	{
		console.error.apply( console, arguments );
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

	export function htmlEncode(txt:string|string[]) : string
	{
		// TODO: Find a better way ?
		if( Array.isArray(txt) )
			return $.map( <string[]>txt, v=>$(document.createElement('span')).text( v ).html() ).join( '<br/>' );
		else
			return $(document.createElement('span')).text( txt ).html();
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

	export function ensureInteger(p:{	observable:KnockoutObservable<Number>,
										fallbackValue?:number,
										mustBePositive?:boolean,
										canBeZero?:boolean,
										canBeNull?:boolean }) : void
	{
		if( p.canBeZero == null )
			p.canBeZero = true;
		if( p.canBeNull == null )
			p.canBeNull = false;
		if( p.fallbackValue == null )
		{
			if( p.canBeZero )
				p.fallbackValue = 0;
			else
				p.fallbackValue = 1;
		}

		let canBeNull = p.canBeNull;
		p.observable.subscribe( function(value:number)
			{
				if( canBeNull )
				{
					if( (<any>value == '') || (value == null) )
					{
						p.observable( null );
						return;
					}
				}

				var newValue = parseInt( <any>value );

				if( isNaN(newValue) )
					newValue = p.fallbackValue;
				else if( (!p.canBeZero) && (value == 0) )
					newValue = p.fallbackValue;
				else if( (p.mustBePositive) && (value < 0) )
					newValue = p.fallbackValue;

				if( value !== newValue )
					// Value must be changed
					p.observable( newValue );
			} );
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

	export function sleep(ms:number) : Promise<void>
	{
		return new Promise( resolve => setTimeout(resolve, ms) );
	}
} // namespace utils

export namespace html
{
	export function showMessage(msg:string) : void
	{
		// Bootstrap style:
		const html = `<div class="modal" tabindex="-1" role="dialog">
						<div class="modal-dialog" role="document">
							<div class="modal-content">
								<div class="modal-body">
									<!-- Text content here -->
								</div>
								<div class="modal-footer">
									<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
								</div>
							</div>
						</div>
					</div>`;
		const $div = $(html);
		$div.find( '.modal-body' ).text( msg );
		$div.modal();

		// JQuery style:
		// var $div = $('<div/>').text('Hello world');
		// $('body').append( $div );
		// $div.dialog()
	}

	export function confirm(msg:string) : Promise<boolean>
	{
		// Bootstrap style:
		const html = `<div class="modal" tabindex="-1" role="dialog">
						<div class="modal-dialog" role="document">
							<div class="modal-content">
								<div class="modal-body">
									<!-- Text content here -->
								</div>
								<div class="modal-footer">
									<button type="button" class="btn btn-primary">Ok</button>
									<button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
								</div>
							</div>
						</div>
					</div>`;
		const $div = $(html);
		$div.find( '.modal-body' ).text( msg );
		return new Promise<boolean>( (resolve)=>
			{
				let confirmed = false;
				$div.find( '.btn-primary' ).click( ()=>
					{
						confirmed = true;
						$div.modal( 'hide' );
						resolve( true );
					} );
				$div.on( 'hidden.bs.modal', (e)=>
					{
						if( confirmed )
							// Clicked on 'Save' => NOOP
							// nb: should not happen, but in case ...
							return;
						// Manually closed => Cancel
						resolve( false );
					} );
				$div.modal();
			} );

		// JQuery style:
		// TODO ...
	}

	/** Invoke jQuery.blockUI's '.block()' on the specified element but supports multiple invokation on the same element */
	export function block($e:JQuery) : JQuery
	{
		// Insert/increment a block counter as jQuery 'data()'
		var blockCounter = ( $e.data('ttt_blockCounter')|0 ) + 1;
		$e.data( 'ttt_blockCounter', blockCounter );

		if( blockCounter == 1 )
			// This element is not blocked yet
			(<any>$e).block();  // TODO: ACA: jQuery.blockUI typings ...

		return $e;
	}

	/** Invoke jQuery.blockUI's '.unblock()' on the specified element except if it has been block()ed more than once */
	export function unblock($e:JQuery) : JQuery
	{
		// Decrement the block counter in the jQuery 'data()'
		var blockCounter = ( $e.data('ttt_blockCounter')|0 ) - 1;
		$e.data( 'ttt_blockCounter', blockCounter );

		if( blockCounter < 0 )
		{
			// There is a logic error somewhere...
			common.utils.error( 'INTERNAL ERROR: Unblock count > block count:', blockCounter );

			// Reset counter
			blockCounter = 0;
			$e.data( 'ttt_blockCounter', 0 );
		}

		if( blockCounter == 0 )
			// This element is no more blocked by anything else
			(<any>$e).unblock();  // TODO: ACA: jQuery.blockUI typings ...

		return $e;
	}

	export function ensureVisible($e:JQuery) : void
	{
		// Scroll
		const offset = $e.offset().top - ( 20 + $e.height() );
		$('html, body').animate({ scrollTop:offset });  // cf.: https://stackoverflow.com/questions/4884839/how-do-i-get-an-element-to-scroll-into-view-using-jquery?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa

		// Blink
		$e.effect( "highlight", {}, 2000 );  // cf.: https://stackoverflow.com/questions/5205445/jquery-blinking-highlight-effect-on-div?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
	}

	export function waitForScrolledVisible($elem:JQuery) : Promise<void>
	{
		const $window = $(window);

		function isScrolledVisible()
		{
			const docViewTop = $window.scrollTop();
			const docViewBottom = docViewTop + $window.height();

			const elemTop = $elem.offset().top;
			const elemBottom = elemTop + $elem.height();
			return ((elemBottom <= docViewBottom) && (elemTop >= docViewTop));
		}

		return new Promise( (resolve)=>
			{
				let scrollHandler : ()=>void;
				scrollHandler = function()
				{
					if(! isScrolledVisible() )
						return;

					// This is a one-shot only => unbind myself
					$window.unbind( 'scroll', scrollHandler );

					resolve();
				}

				if( isScrolledVisible() )
					// Element is visible right now => no need to bind to scroll event
					resolve();
				else  // Bind to scroll event & wait for $elem to be visible
					$window.bind( 'scroll', scrollHandler );
			} );
	}
} // namespace html

export namespace url
{
	const onChangedEvent = 'onUrlChanged';
	var onChangedCallbacks = $({});
	var onChangedCallbacksRegistered = false;

	export function redirect(url:string)
	{
		window.location.href = url;
	}

	/** Parse a parameters string like 'foo=bar&hello=world' and return the a dictionary like {foo:'bar',hello:'world'} */
	export function parseParameters(searchString?:string) : {[key:string]:any}
	{
		if( searchString == null )
			searchString = window.location.search.substring(1);
		if( searchString.length == 0 )
			return {};

		var tokens = searchString.split( '&' );

		var dict = <{[key:string]:any}>{};
		for( var i=0; i < tokens.length; ++i )
		{
			var pairs = tokens[i].split( '=' );
			var key = decodeURIComponent( pairs[0] );
			var value = decodeURIComponent( pairs[1] );

			// Try parse JSON
			try { value = JSON.parse(value); }
			catch( ex ){/* Not JSON => Keep the string as-is*/}

			dict[ key ] = value;
		}
		return dict;
	}

	/** Transform a dictionary like {foo:'bar',hello:'world'} to a parameters string like 'foo=bar&hello=world' */
	export function stringifyParameters(parms:{[key:string]:any}) : string
	{
		var pairs = <string[]>[];
		$.each( parms, function(key:string,value)
			{
				key = encodeURIComponent( key );

				if( (value == null) || (typeof(value) === 'string') || (typeof(value) === 'number') || (typeof(value) === 'boolean') )
					{/*Keep as-is*/}
				else
					// Convert to JSON
					value = JSON.stringify( value );
				value = encodeURIComponent( value );

				pairs.push( key+"="+value );
			} );
		return pairs.join( '&' );
	}

	/** Registers a callback to be invoked whenever the browser's URL changes (cf. 'pushHistory()') */
	export function onChanged(callback:()=>void) : void
	{
		// NB: The callback gets invoked ALSO when 'pushHistory()' is invoked

		if( onChangedCallbacksRegistered == false )
		{
			$(window).bind( 'popstate', function(evt)
				{
					onChangedCallbacks.trigger( onChangedEvent );
				} );
			onChangedCallbacksRegistered = true;
		}

		onChangedCallbacks.bind( onChangedEvent, callback );
	}

	export function createUrl(p:{ pathname?:string, parameters?:{[key:string]:any} }) : string
	{
		var newPath = p.pathname;
		if( newPath == null )
			newPath = window.location.pathname;

		if( p.parameters != null )
		{
			const queryString = stringifyParameters( p.parameters );
			if( queryString.length > 0 )
				newPath = newPath+'?'+queryString;
		}
		return newPath;
	}

	/** Change the browser's current URL without triggering an HTTP request (NB: Will trigger any registered 'onChanged()' callbacks) */
	export function pushHistory(p:{ pathname?:string, parameters?:{[key:string]:any}, newTitle?:string }) : void
	{
		const newPath = createUrl( p );
		window.history.pushState( {}, p.newTitle, newPath );

		// Invoke any registered callbacks
		onChangedCallbacks.trigger( onChangedEvent );
	}

	export function postRequestForm<T>(url:string, request:{[key:string]:any}) : Promise<T>
	{
		utils.log( 'postRequestForm', { url, request } );
		return new Promise<T>( (resolve,reject)=>
			{
				$.ajax({	type		: 'POST',
							url			: url,
							contentType	: 'application/x-www-form-urlencoded',
							data		: request,
							dataType	: 'json',
							success		: (data,textStatus,jqXHR)=>
											{
												utils.log( 'postRequestForm', { response:data } );
												resolve( data );
											},
							error		: (jqXHR,textStatus,errorThrown)=>
											{
												utils.error( 'postRequestForm rejected', { jqXHR, textStatus, errorThrown } );
												reject( textStatus );
											}
						});
			} );
	}

	export function postRequestFormData<T>(url:string, formData:FormData) : Promise<T>
	{
		utils.log( 'postRequestFormData', { url, formData } );
		return new Promise<T>( (resolve,reject)=>
			{
				$.ajax( {	type		: "POST",
							url			: url,
							contentType	: false,
							processData	: false,
							data		: formData,
							dataType	: 'json',
							success		: (data:T,textStatus,jqXHR)=>
											{
												utils.log( 'postRequestFormData', { response:data } );
												resolve( data );
											},
							error		: (jqXHR,textStatus,errorThrown)=>
											{
												utils.error( 'postRequestFormData rejected', { jqXHR, textStatus, errorThrown } );
												reject( textStatus );
											},
						} );
			} );
	}

	export function postRequestJSON<T>(url:string, request:{[key:string]:any}) : Promise<T>
	{
		utils.log( 'postRequestJSON', { url, request } );
		let requestStr = JSON.stringify( request );
		return new Promise<T>( (resolve,reject)=>
			{
				$.ajax({	type		: 'POST',
							url			: url,
							contentType	: 'application/json',
							data		: requestStr,
							dataType	: 'json',
							success		: (data,textStatus,jqXHR)=>
											{
												utils.log( 'postRequestJSON', { response:data } );
												resolve( data );
											},
							error		: (jqXHR,textStatus,errorThrown)=>
											{
												utils.error( 'postRequestJSON rejected', { jqXHR, textStatus, errorThrown } );
												reject( textStatus );
											}
						});
			} );
	}

	export function getRequest(url:string, request?:{[key:string]:any}) : Promise<string>
	{
		if( request != null )
		{
			const parms = stringifyParameters( request );
			url = `${url}?${parms}`;
		}
		utils.log( 'getRequest', { url, request } );

		return new Promise<string>( (resolve,reject)=>
			{
				$.ajax({	type		: 'GET',
							url			: url,
							contentType	: 'text/html',
							success		: (data,textStatus,jqXHR)=>
											{
												utils.log( 'getRequest', { response:data } );
												resolve( data );
											},
							error		: (jqXHR,textStatus,errorThrown)=>
											{
												utils.error( 'getRequest rejected', { jqXHR, textStatus, errorThrown } );
												reject( textStatus );
											}
						});
			} );
	}

	export function downloadFile(url:string) : void
	{
		$(document.body).append( $("<iframe/>").attr({	src		: url,
														style	: 'visibility:hidden;display:none' })
								);
	}

} // namespace url

// nb: Exports at the end or the order of execution breaks everything (i.e. strEnum must be defined before) ...
export * from "../Language";
export * from "../PageHelper";
export * from "../Routes";
