
import * as common from "../Views/common"
import * as ctrl from "./TreeController";

export const types =
{
	html			: 'html',
	translatedHtml	: 'translatedHtml',
	image			: 'image',
	view			: 'view',
}

const pageManagerTemplate = `
<div class="tree-pagemanager">
	This is should be in top-right corner<br/>
	<button data-bind="click:save, text:saveButtonText, css:{ 'btn-primary':(hasChanges()), 'btn-secondary':(!hasChanges()) }" class="btn"/>
</div>
`;

export class PageManager
{
	public readonly	$container			: JQuery;
	public readonly	$blockingDiv		: JQuery;
	public readonly	trackedComponents	: KnockoutObservableArray<Component>;
	public readonly	hasChanges			: KnockoutComputed<boolean>;
	public readonly	saveButtonText		: KnockoutObservable<string>;

	constructor($container:JQuery)
	{
		const self = this;
		this.$container			= $container;
		this.$blockingDiv		= $('body');
		this.trackedComponents	= ko.observableArray();
		this.hasChanges			= ko.computed( ()=>
									{
										for( const obj of self.trackedComponents() )
											if( obj.trackedObject.hasChanges() )
												return true;
										return false;
									} );
		this.saveButtonText	= ko.observable( 'Save' );
	}

	protected async save() : Promise<void>
	{
		common.utils.log( 'PageManager.save()' );
		const self = this;

		const operations : ctrl.operations.Operation[] = [];
		const components : Component[] = [];
		for( const component of self.trackedComponents() )
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
		for( let i=0; i<components.length; ++i )
		{
			const component = components[ i ];
			const response = responses[ i ];
			component.setOperationResponse( response );
			component.trackedObject.setInitial();
		}
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

export interface Component
{
	trackedObject : common.utils.TrackedObservable<any>;

	getOperation() : Promise<ctrl.operations.Operation>;
	setOperationResponse(response:ctrl.operations.Response) : Promise<void>;
}
