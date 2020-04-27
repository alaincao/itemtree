
import * as tree from "./TreeHelper";
import * as ctrl from "./TreeController";
import * as lng from "../Language";
import * as common from "../Views/common"
import { addKoTinymceEditor } from "../Views/Blog/Edit";

const attributePath			= "tree-path";
const attributeType			= "tree-type";
const attributeParameters	= "tree-parameters";

export function init()
{
	common.utils.log( 'tree.init()' );

	const callbacks : {[key:string]:($e:JQuery,path:string)=>void} = {};
	callbacks[ tree.types.html ]			= HtmlComponent.create;
	callbacks[ tree.types.translatedHtml ]	= HtmlTranslatedComponent.create;
	callbacks[ tree.types.image ]			= initImage;

	common.utils.log( 'tree.init(): Initialize components' );

	const elements = $(`[${attributeType}]`);
	for( let i=0; i<elements.length; ++i  )
	{
		const $element = $(elements[i]);
		const type = $element.attr( attributeType );
		const path = $element.attr( attributePath );
		const callback = callbacks[ type ];
		if( callback == null )
			continue;

		callback( $element, path );
	}

	common.utils.log( 'tree.init(): End' );
}

//////////

class HtmlComponent implements tree.PageComponent
{
	private readonly	pageManager		: tree.PageManager;
	private readonly	path			: string;
	public readonly		trackedObject	: common.utils.TrackedObservable<string>;

	private constructor(pageManager:tree.PageManager, path:string)
	{
		this.pageManager = pageManager;
		this.path = path;
		this.trackedObject = common.utils.observable();
	}

	public static async create($element:JQuery, path:string) : Promise<void>
	{
		common.utils.log( 'tree-html', { $element, path } );

		// Add requirements
		addKoTinymceEditor();

		// Create new instance & register against the PageManager
		const pageManager = tree.getPageManager();
		const self = new HtmlComponent( pageManager, path );
		pageManager.registerComponent( self );

		// Reset element's content using value from server
		await self.refresh();  // nb: Need to refresh BEFORE applying KO's bindings: If the element is empty, TinyMCE's initial status will be buggy ...

		// Define bindings
		$element.attr( 'data-bind', 'tinymceEditor:trackedObject' );
		ko.applyBindings( self, $element[0] );
	}

	private async refresh() : Promise<void>
	{
		common.utils.log( 'HtmlComponent.refresh()' );
		const self = this;

		// Retreive value from server & update observable
		common.html.block( self.pageManager.$blockingDiv );
		try
		{
			var value = await common.url.getRequest( self.path );
		}
		catch( ex )
		{
			common.utils.error( `Retreive of path '${self.path}' failed: ${ex}` );
			value = '<p></p>';
		}
		finally
		{
			common.html.unblock( self.pageManager.$blockingDiv );
		}
		self.trackedObject( value );
		self.trackedObject( 'value' );
		self.trackedObject( value );
		self.trackedObject.setInitial();  // ie. reset 'hasChanges'
	}

	public async getOperation(): Promise<ctrl.operations.Operation>
	{
		const self = this;

		const operation : ctrl.operations.GetOrCreateNode = {
				path			: self.path,
				expectedType	: tree.types.html,
				data			: self.trackedObject(),
			};
		return{ getOrCreateNode:operation };
	}

	public async setOperationResponse(response:ctrl.operations.Response): Promise<void>
	{
		const self = this;
		await self.refresh();
	}
}

//////////

class HtmlTranslatedComponent implements tree.PageComponent
{
	private readonly	pageManager		: tree.PageManager;
	private readonly	path			: string;
	private readonly	translation		: lng.Translation;
	public readonly		current			: KnockoutObservable<string>;
	public readonly		trackedObject	: common.utils.TrackedObservable<{[key in lng.Language]:string}>;

	private constructor(pageManager:tree.PageManager, path:string)
	{
		const self = this;
		this.pageManager = pageManager;
		this.path = path;

		this.translation		= new lng.Translation( self.pageManager.getCurrentLanguageKO() );
		this.current			= self.translation.current;
		this.trackedObject		= self.translation.trackable;
	}

	public static async create($element:JQuery, path:string) : Promise<void>
	{
		common.utils.log( 'tree-htmlTranslated', { $element, path } );

		// Add requirements
		addKoTinymceEditor();

		// Create new instance & register against the PageManager
		const pageManager = tree.getPageManager();
		const self = new HtmlTranslatedComponent( pageManager, path );
		pageManager.registerComponent( self );

		// Reset element's content using value from server
		await self.refresh();  // nb: Need to refresh BEFORE applying KO's bindings: If the element is empty, TinyMCE's initial status will be buggy ...

		// Define bindings
		$element.attr( 'data-bind', 'tinymceEditor:current' );
		ko.applyBindings( self, $element[0] );
	}

	private async refresh() : Promise<void>
	{
		common.utils.log( 'HtmlTranslatedComponent.refresh()' );
		const self = this;

		// Retreive value from server & update observable
		common.html.block( self.pageManager.$blockingDiv );
		let values : {[key in lng.Language]:string};
		try
		{
			const rv = await ctrl.operations([ {getNodeData:{path:self.path}} ]);
			if(! rv.success )
			{
				common.utils.error( 'GetNodeData operation failed', { rv } );
				common.html.showMessage( rv.errorMessage );
				return;
			}
			const json = rv.responses[0].data;
			values = JSON.parse( json );
		}
		catch( ex )
		{
			common.utils.error( `Retreive of path '${self.path}' failed: ${ex}` );
			self.translation.resetValues();
		}
		finally
		{
			common.html.unblock( self.pageManager.$blockingDiv );
		}
		self.translation.resetValues( values );
	}

	public async getOperation(): Promise<ctrl.operations.Operation>
	{
		const self = this;

		const operation : ctrl.operations.GetOrCreateNode = {
				path			: self.path,
				expectedType	: tree.types.translatedHtml,
				data			: self.translation.getValues(),
			};
		return{ getOrCreateNode:operation };
	}

	public async setOperationResponse(response:ctrl.operations.Response): Promise<void>
	{
		const self = this;
		await self.refresh();
	}
}

//////////

async function initImage($element:JQuery, path:string) : Promise<void>
{
	common.utils.log( 'tree-image: init', { $element, path } );

	$element.click( ()=>
		{
			common.utils.log( 'tree-image: Click', {$element} );

			const $input = $('<input type="file" />');
			const input = <HTMLInputElement>$input[0];
			$input.change( async ()=>
				{
					if( input.files.length < 1 )
						// No file selected
						return;
					const file = input.files[0];
					const $blockingDiv = $element.parent();

					common.utils.log( 'tree-image: Post file' );
					common.html.block( $blockingDiv );
					let rc : ctrl.ImageSaveResult;
					try
					{
						rc = await ctrl.imageSave( path, file );
					}
					finally
					{
						common.html.unblock( $blockingDiv );
					}
					if(! rc.success )
					{
						common.utils.error( 'Image upload error', { rc } );
						common.html.showMessage( rc.errorMessage );
						return;
					}

					let src = rc.path;
					const parms = common.url.parseParameters( $element.attr(attributeParameters) ?? '' );
					parms[ 'refresh' ] = ''+Date.now();  // nb: to bypass browser's cache
					src = src+'?'+common.url.stringifyParameters( parms );
					common.utils.log( 'tree-image: Refresh image', {src} );
					$element.attr( 'src', src );
				} );
			$input.click();
		} );
}
