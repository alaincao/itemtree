
import { newGuid, strEnum, jsonParse } from "../Views/utils";
import * as common from "../Views/common"
import * as ctrl from "./TreeController";
import * as lng from "../Language";

export const { e:Types, a:allTypes } = strEnum([
												'html',
												'translatedHtml',
												'file',
												'image',
												'view',

												'pageProperty',
											]);
export type Type = keyof typeof Types;

const pageManagerTemplate = `
<div class="tree-pagemanager">
	<ul data-bind='foreach:pageProperties'>
		<li data-bind="template:templateID"></li>
	</ul>
	<button data-bind="click:save, css:{ 'btn-primary':(hasChanges()), 'btn-secondary':(!hasChanges()) }" class="btn">Save</button>
</div>
`;

const currentLanguageTemplate = `
	<strong>Current language:</strong>
	<select data-bind="options:allLanguages, value:current"></select>
`;


//////////

export interface PageComponent
{
	trackedObject : common.utils.TrackedObservable<any>;

	getOperation() : Promise<ctrl.operations.Operation>;
	setOperationResponse(response:ctrl.operations.Response) : Promise<void>;
}

export class PageProperty
{
	public readonly	templateID	: string;
	public readonly	template	: string;

	constructor(template:string)
	{
		this.template = template;

		let templateID = common.html.getKoTemplateID( template );
		if( templateID != null )
		{
			// This template has already been registered => NOOP
		}
		else  // This template needs an ID and be registered
		{
			templateID = '_'+newGuid();
			common.html.registerKoTemplate( templateID, template );
		}
		this.templateID = templateID;
	}
}

//////////

export class PageManager
{
	public readonly		$container		: JQuery;
	public readonly		$blockingDiv	: JQuery;
	private readonly	pageComponents	: KnockoutObservableArray<PageComponent>;
	protected readonly	pageProperties	: KnockoutObservableArray<PageProperty>;
	public readonly		hasChanges		: KnockoutComputed<boolean>;

	private				currentLanguage	: CurrentLanguageProperty = null;

	constructor($container:JQuery)
	{
		const self = this;
		this.$container		= $container;
		this.$blockingDiv	= $('body');
		this.pageComponents	= ko.observableArray();
		this.pageProperties	= ko.observableArray();
		this.hasChanges		= ko.pureComputed( ()=>
								{
									for( const obj of self.pageComponents() )
										if( obj.trackedObject.hasChanges() )
											return true;
									return false;
								} );
	}

	public async registerComponent(component:PageComponent) : Promise<void>
	{
		const self = this;
		self.pageComponents.push( component );
	}

	public async registerProperty(property:PageProperty) : Promise<void>
	{
		const self = this;
		self.pageProperties.push( property );
	}

	public getCurrentLanguageKO() : KnockoutObservable<lng.Language>
	{
		const self = this;
		if( self.currentLanguage == null )
		{
			self.currentLanguage = new CurrentLanguageProperty();
			self.registerProperty( self.currentLanguage );
		}
		return self.currentLanguage.current;
	}

	public getNodeMemberKO(path:string, member:string) : Promise<KnockoutObservable<any>>
	{
		return NodeContentComponent.getNodeMemberKO( path, member );
	}

	protected async save() : Promise<void>
	{
		common.utils.log( 'PageManager.save()' );
		const self = this;

		const operations : ctrl.operations.Operation[] = [];
		const components : PageComponent[] = [];
		for( const component of self.pageComponents() )
		{
			if(! component.trackedObject.hasChanges() )
				continue;
			const op = await component.getOperation();
			if( op == null )
				continue;
			components.push( component );
			operations.push( op );
		}

		if( operations.length == 0 )
		{
			common.utils.log( 'PageManager.save(): Nothing to do' );
			return;
		}

		common.html.block( self.$blockingDiv );
		let responses : ctrl.operations.Response[];
		try
		{
			const rc = await ctrl.operations( operations );
			if(! rc.success )
			{
				common.utils.error( 'Save operations failed', { rc } );
				common.html.showMessage( rc.errorMessage );
				return;
			}
			responses = rc.responses;
		}
		finally
		{
			common.html.unblock( self.$blockingDiv );
		}

		common.utils.log( 'Send responses to components', { components, responses:responses } );
		const tasks : Promise<void>[] = [];
		for( let i=0; i<components.length; ++i )
		{
			const component = components[ i ];
			const response = responses[ i ];
			tasks.push( (async ()=>
				{
					await component.setOperationResponse( response );
					component.trackedObject.setInitial();
				} )() );
		}

		common.utils.log( 'Wait for tasks to terminate' );
		await Promise.all( tasks );
	}
}
var pageManagerInstance : PageManager = null;

export function getPageManager() : PageManager
{
	if( pageManagerInstance != null )
		return pageManagerInstance;
	common.utils.log( 'getPageManager()' );

	const $container = $(pageManagerTemplate);
	$('body').append( $container );
	pageManagerInstance = new PageManager( $container );
	ko.applyBindings( pageManagerInstance, $container[0] );
	return pageManagerInstance;
}

//////////

class CurrentLanguageProperty extends PageProperty
{
	protected readonly	allLanguages	: KnockoutObservableArray<string>;
	public readonly		current			: KnockoutObservable<lng.Language>;

	constructor()
	{
		super( currentLanguageTemplate );
		this.allLanguages	= ko.observableArray( common.allLanguages );
		this.current		= ko.observable( common.pageParameters.currentLanguage );
	}
}

//////////

class NodeContentComponent implements PageComponent
{
	private static	trackedNodes	: {[path:string]:NodeContentComponent};

	private readonly	path			: string;
	public readonly		trackedObject	: common.utils.TrackedObservable<{[key:string]:any}>;

	constructor(path:string, initialValue:{[key:string]:any})
	{
		this.path			= path;
		this.trackedObject	= common.utils.observable( ko.observable(initialValue) );
	}

	public static async getNodeMemberKO(path:string, member:string) : Promise<KnockoutObservable<any>>
	{
		if( NodeContentComponent.trackedNodes == null )
			NodeContentComponent.trackedNodes = {};

		let instance = NodeContentComponent.trackedNodes[ path ];
		if( instance == null )
		{
			// Path not yet tracked => Create instance

			let json = await ctrl.getNodeData( path );
			if( json != null )
				var initialValue = <{[key:string]:any}>jsonParse( json );
			else
				initialValue = {};
			instance = new NodeContentComponent( path, initialValue );

			NodeContentComponent.trackedNodes[ path ] = instance;
			getPageManager().registerComponent( instance );
		}

		// Return an observable that tracks only a single member of the 'trackedObject'
		const o = instance.trackedObject;
		const rv = ko.pureComputed({
						read : ()=>o()[ member ],
						write : (value)=>
							{
								const a = o();
								a[ member ] = value;
								o( a );
							},
					})
		return rv;
	}

	public async getOperation(): Promise<ctrl.operations.Operation>
	{
		const self = this;

		const operation : ctrl.operations.SetNodeData = {
				path	: self.path,
				data	: self.trackedObject(),
			};
		return{ setNodeData:operation };
	}

	public async setOperationResponse(response: ctrl.operations.Response): Promise<void>
	{
		const self = this;

		// Refresh data from server
		const json = await ctrl.getNodeData( self.path );
		self.trackedObject( jsonParse(json) );
		self.trackedObject.setInitial();
	}
}
