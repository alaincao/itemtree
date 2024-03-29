
import { jsonParse, jsonStringify } from "./utils";
import * as common from "./common";
import { PageParameters } from "../PageHelper";
import { Routes } from "../Routes";
import * as tree from "../Tree/TagHelpers";


export var pageParameters : PageParameters;
export var routes : Routes;

export function init(p:{ pageParameters:PageParameters })
{
	pageParameters = p.pageParameters;
	routes = pageParameters.routes;
	utils.log( 'common.init()' );

	if( pageParameters.hasErrors )
	{
		utils.log( 'common.init(): Page has errors! ; outputting the logs' );
		common.utils.error( 'Page has errors!', { log:pageParameters.logs } );
	}

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

	ko.bindingHandlers.ttt_blink =
		{
			init: (element,valueAccessor)=>
					{
						// Initially hidden
						$(element).hide();

						// Initial value must always be 'false'
						const koValue = valueAccessor();
						koValue( false );
					},
			update: (element,valueAccessor)=>
					{
						const $element = $(element);
						const koValue = valueAccessor();
						if( koValue() == true )
						{
							// Switched to 'true' => animate
							$element.fadeIn().delay(1000).fadeOut( ()=>
								{
									// then revert to 'false'
									koValue( false );
								} );
						}
					},
		};

	// https://stackoverflow.com/questions/16930869/how-to-access-file-input-with-knockout-binding
	ko.bindingHandlers.ttt_file =
		{
			init: (element,valueAccessor)=>
					{
						const unwrapped = valueAccessor()
						ko.utils.registerEventHandler( element, "change", ()=>
							{
								unwrapped( element.files );
							} );
					}
		};

	utils.log( 'common.init(): Replace Knockout template engine' );
	{
		ko.setTemplateEngine( html.newKoTemplateSource() );
	}

	utils.log( 'common.init(): Initialize tree framework' );
	tree.init();
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

	/** Function to enforce field name string at compile-time
	 * cf. https://stackoverflow.com/questions/50470025/nameof-keyword-in-typescript */
	export const nameof = <T>(name: Extract<keyof T, string>): string => name;

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

	export function arrayUnique<T>(a:T[]) : T[]
	{
		return a.filter( (v,i)=>( a.indexOf(v) === i ) );
		//return [ ... new Set(a) ];
	}

	export function arrayRemoveItem<T>(a:T[], item:T) : boolean
	{
		let i = a.indexOf( item );
		if( i < 0 )
			return false;
		a.splice( i, 1 );
		return true;
	}

	export function arrayInsertAfter<T>(a:T[], item:T, afterItem:T) : void
	{
		const idx = a.indexOf( afterItem ) + 1;  // nb: Will insert @ index 0 if not found
		a.splice( idx, 0, item );
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
		const observable		= p.observable;
		const mustBePositive	= (p.mustBePositive == null) ? false : p.mustBePositive;
		const canBeZero			= (p.canBeZero == null) ? true : p.canBeZero;
		const canBeNull			= (p.canBeNull == null) ? false : p.canBeNull;
		const fallbackValue		= (p.fallbackValue != null) ? p.fallbackValue : (canBeZero) ? 0 : 1;

		observable.subscribe( function(value:number)
			{
				if( canBeNull )
				{
					if( (<any>value == '') || (value == null) )
					{
						observable( null );
						return;
					}
				}

				var newValue = parseInt( <any>value );

				if( isNaN(newValue) )
					newValue = fallbackValue;
				else if( (!canBeZero) && (value == 0) )
					newValue = fallbackValue;
				else if( (mustBePositive) && (value < 0) )
					newValue = fallbackValue;

				if( value !== newValue )
					// Value must be changed
					observable( newValue );
			} );
	}

	export function ensureString(p:{	observable	: KnockoutObservable<string>,
										canBeNull?	: boolean,
								}) : void
	{
		const observable	= p.observable;
		const canBeNull		= (p.canBeNull == null) ? true : p.canBeNull;

		observable.subscribe( (value:any)=>
			{
				if( typeof(value) == 'string' )
					// OK
					return;
				if( canBeNull && (value === null) )
					// OK
					return;
				// KO => Force as string
				observable( ''+value );
			} );
	}

	export function ensureBoolean(p:{	observable		: KnockoutObservable<boolean>,
										canBeNull?		: boolean,
										fallbackValue?	: boolean,
								}) : void
	{
		const observable	= p.observable;
		const canBeNull		= (p.canBeNull == null) ? false : p.canBeNull;
		const fallbackValue	= (p.fallbackValue == null) ? false : p.fallbackValue;

		observable.subscribe( (value:any)=>
			{
				if( typeof(value) == 'boolean' )
					// OK
					return;
				if( canBeNull && (value === null) )
					// OK
					return;
				// KO => Force as string
				observable( fallbackValue );
			} );
	}

	export interface TrackedObservable<T> extends KnockoutObservable<T>
	{
		hasChanges		: KnockoutObservable<boolean>;
		setInitial() : void;
		reset() : void;
	}
	export function observable<T>(o?:KnockoutObservable<T>) : TrackedObservable<T>
	{
		if( o == null )
			o = ko.observable<T>();

		const obj = <TrackedObservable<T>>o;
		const originalValue	= ko.observable( jsonStringify(obj()) );
		(<any>obj)['originalValue'] = originalValue;  // nb: For debugging usage only: Render the 'originalValue' available to the JS console, but do not expose it to the TypeScript definitions
		obj.hasChanges	= ko.pureComputed( ()=> jsonStringify(obj()) != originalValue() );
		obj.setInitial	= ()=>originalValue( jsonStringify(obj()) );
		obj.reset		= ()=>obj( jsonParse(originalValue()) );
		return obj;
	}
} // namespace utils

export namespace html
{
	export function showMessage(msg:string) : void
	{
		// Bootstrap style:
		const html = `<div class="modal" tabindex="-1" role="dialog">
						<div class="modal-dialog modal-dialog-centered" role="document">
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

	export function showHtml(content:string) : void
	{
		// Bootstrap style:
		const html = `<div class="modal" tabindex="-1" role="dialog">
						<div class="modal-dialog modal-dialog-centered" role="document">
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
		$div.find( '.modal-body' ).html( content );
		$div.modal();
	}

	export function confirm(msg:string) : Promise<boolean>
	{
		// Bootstrap style:
		const html = `<div class="modal" tabindex="-1" role="dialog">
						<div class="modal-dialog modal-dialog-centered" role="document">
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
				$div.find( '.btn-primary' ).on( 'click', ()=>
					{
						// 'Ok' clicked
						confirmed = true;
						$div.modal( 'hide' );
						$div.modal( 'dispose' );
					} );
				$div.on( 'hidden.bs.modal', (e)=>
					{
						// Dialog closed
						resolve( confirmed );
					} );
				$div.modal();
			} );

		// JQuery style:
		// TODO ...
	}

	/**
	 * WARNING: Function does not return when no file is chosen (ie. resolve() not invoked)
	 */
	export function showFileSelector() : Promise<File>
	{
		return new Promise<File>( (resolve)=>
			{
				const $input = $('<input type="file" />');
				const input = <HTMLInputElement>$input[0];
				$input.on( 'change', ()=>
					{
						if( input.files.length < 1 )
						{
							resolve( null );
							return;
						}
						const file = input.files[0];
						if( file == null )
						{
							resolve( null );
							return;
						}
						resolve( file );
					} );
				$input.trigger( 'click' );
			} );
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

	// based on https://embed.plnkr.co/plunk/yfQAnM
	var registeredTemplates : {[name:string]:string};
	export function registerKoTemplate(name:string, content:string) : void
	{
		registeredTemplates[ name ] = content;
	}
	export function getKoTemplateID(content:string) : string
	{
		for( const id of Object.keys(registeredTemplates) )
		{
			if( registeredTemplates[id] === content )
				return id;
		}
		return null;
	}
	export function newKoTemplateSource() : KnockoutNativeTemplateEngine
	{
		if( registeredTemplates == null )
			registeredTemplates = {};

		const engine = new ko.nativeTemplateEngine();
		engine.makeTemplateSource = (template, doc)=>
			{
				if( typeof(template) === 'string' )
				{
					// Try get an element that has this ID
					const elem = (doc || document).getElementById( template );
					if( elem != null )
						return new ko.templateSources.domElement( elem );

					// Failed => Fallback to 'StringTemplateSource'
					return new StringTemplateSource( template );
				}
				else if (template && (template.nodeType == 1) || (template.nodeType == 8))
				{
					return new ko.templateSources.anonymousTemplate(template);
				}
			};
		return engine;
	}
	class StringTemplateSource
	{
		public readonly		name	: string;
		private readonly	data_	: {[key:string]:any};
		constructor(name:string)
		{
			this.name	= name;
			this.data_	= {};
		}
		public text(value:string) : string|void
		{
			const self = this;

			if( arguments.length === 0 )
				return registeredTemplates[ self.name ];
			registerKoTemplate( self.name, value );
			return;
		}
		public data(key:string, value:any) : any|void
		{
			const self = this;

			if( arguments.length === 1 )
				return self.data_[ key ];
			else
				self.data_[key] = value;
		}
	}
} // namespace html

export namespace url
{
	const onChangedEvent = 'onUrlChanged';
	var onChangedCallbacks = $({});
	var onChangedCallbacksRegistered = false;

	export function refresh()
	{
		window.location.reload();
	}

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
			try { value = jsonParse(value); }
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
					value = jsonStringify( value );
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
		let requestStr = jsonStringify( request );
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
												reject( errorThrown );
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
