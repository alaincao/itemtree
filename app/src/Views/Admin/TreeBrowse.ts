import { jsonParse, jsonStringify } from "../utils";
import * as common from "../common";
import * as ctrl from "../../Tree/TreeController";

export var $blockingDiv			: JQuery;
export var rootNode				: Node;
export var nodeDetailsDialog	: NodeDetailsDialog;
export var nodeUploadDialog		: NodeUploadDialog;

export async function init(p:{
								$blockingDiv				: JQuery,
								$nodeDetailsDialogTemplate	: JQuery,
								$nodeUploadDialogTemplate	: JQuery,
							}) : Promise<void>
{
	$blockingDiv	= p.$blockingDiv;

	rootNode			= new Node( ctrl.pathSeparator );
	nodeDetailsDialog	= new NodeDetailsDialog( p.$nodeDetailsDialogTemplate );
	nodeUploadDialog	= new NodeUploadDialog( p.$nodeUploadDialogTemplate );

	await rootNode.setExpanded( /*value*/true, /*recursive*/false );  // Expand 1st level
}

class Node
{
	public readonly	path			: string;
	public readonly	downloadPath	: string;
	public readonly	segments		: string[];
	public readonly	name			: string;
	public readonly type			: KnockoutObservable<string>;
	public readonly	children		: KnockoutObservableArray<Node>;
	public readonly	hasChildren		: KnockoutReadonlyComputed<boolean>;
	public readonly	isExpanded		: KnockoutReadonlyComputed<boolean>;

	constructor(path:string)
	{
		const self = this;
		this.path			= path;
		this.downloadPath	= ctrl.getDownloadUrl({ path:path, excludeImages:false, forDownload:true });
		this.segments		= ctrl.getPathSegments( path );
		this.name			= (self.segments.length == 0) ? /*root*/ctrl.pathSeparator : self.segments[ self.segments.length-1 ];
		this.type			= ko.observable( null );
		this.children		= ko.observableArray(); self.children( null );
		this.hasChildren	= ko.pureComputed({ read:()=>self.children()?.length > 0 });
		this.isExpanded		= ko.pureComputed({ read:()=>self.children() != null });
	}

	public async toggleExpanded() : Promise<void>
	{
		await this.setExpanded( ! this.isExpanded() );
	}

	public async setExpanded(value:boolean, recursive?:boolean) : Promise<void>
	{
		const self = this;
		recursive = (recursive == null) ? false : recursive;

		if( value == false )
		{
			// Just ensure the children list is empty
			self.children( null );
			return;
		}

		const tasks : Promise<any>[] = [];

		if( self.isExpanded() == false )
		{
			const newChildren = await self.refreshChildren();
			const childTasks = newChildren.map( (child)=>child.refresh(/*recursive*/false) );
			tasks.push.apply( tasks, childTasks );
		}

		if( recursive )
		{
			const childTasks = self.children().map( (child)=>child.setExpanded(/*value*/true, /*recursive*/true) );
			tasks.push.apply( tasks, childTasks );
		}

		await Promise.all( tasks );
	}

	public async refresh(recursive?:boolean) : Promise<void>
	{
		const self = this;
		recursive = (recursive == null) ? false : recursive;

		common.html.block( $blockingDiv );
		try
		{
			const tasks : Promise<any>[] = [];

			// Update node's type
			tasks.push( (async ()=>
				{
					const meta = await ctrl.getNodeMetaData( self.path );
					if( meta == null )
						self.type( null );
					else
						self.type( meta['type'] ?? null );
				})() );

			if( self.isExpanded() == true )
			{
				// Refresh children
				tasks.push( (async ()=>
					{
						const children = await self.refreshChildren();
						if( recursive == true )
						{
							const childTasks = children.map( child=>child.refresh(true) );
							await Promise.all( childTasks );
						}
					})() );
			}

			await Promise.all( tasks );
		}
		finally
		{
			common.html.unblock( $blockingDiv );
		}
	}

	/**
	 * @returns the list of new child nodes
	 */
	public async refreshChildren() : Promise<Node[]>
	{
		const self = this;

		common.html.block( $blockingDiv );
		try
		{
			const origs : {[key:string]:Node} = {};
			for( const node of self.children()??[] )
				origs[ node.name ] = node;

			const newChildren : Node[] = [];
			const childNodes = await ctrl.getChildNodes( self.path );
			const children = childNodes.map( childName=>
				{
					let childNode = origs[ childName ];
					if( childNode == null )
					{
						// Create new
						childNode = new Node( ctrl.pathSeparator+self.segments.concat(childName).join(ctrl.pathSeparator) );
						newChildren.push( childNode );
					}
					return childNode
				} );
			self.children( children );

			return newChildren;
		}
		finally
		{
			common.html.unblock( $blockingDiv );
		}
	}

	public async showMetadata() : Promise<void>
	{
		await nodeDetailsDialog.show( this );
	}

	public async delTree() : Promise<void>
	{
		const self = this;
		common.utils.log( `delTree: START` );

		common.html.block( $blockingDiv );
		try
		{
			common.utils.log( `delTree: Show confirmation dialog` );
			const confirmed = await common.html.confirm(`Delete node at '${self.path}' and all it's children?`)
			common.utils.log( `delTree: Confirmed: '${confirmed}'` );
			if( ! confirmed )
				return;

			common.utils.log( `delTree: Invoke 'delTree'` );
			await ctrl.delTree( self.path, /*included*/true );

			common.utils.log( `delTree: Invoke 'refresh'` );
			await rootNode.refresh( /*recursive*/true );
		}
		catch( err )
		{
			common.utils.error( `delTree() threw an error`, err );
			common.html.showMessage( ''+err );
		}
		finally
		{
			common.html.unblock( $blockingDiv );
		}
		common.utils.log( `delTree: END` );
	}

	public async showUpload() : Promise<void>
	{
		await nodeUploadDialog.show( this );
	}
}

class NodeDetailsDialog
{
	private readonly	$dialog				: JQuery;
	private readonly	$blockingDiv		: JQuery;
	private				node				: Node;
	private readonly	metadataInitial		: KnockoutObservable<string>;
	protected readonly	metadata			: KnockoutComputed<string>;
	private readonly	metadataDict		: KnockoutObservable<{[key:string]:any}>;
	protected readonly	metadataChanged		: KnockoutObservable<boolean>;
	private readonly	dataInitial			: KnockoutObservable<string>;
	protected readonly	data				: KnockoutObservable<string>;
	protected readonly	dataChanged			: KnockoutObservable<boolean>;

	constructor($template:JQuery)
	{
		common.utils.log( `NodeDetailsDialog: constructor` );
		const self = this;
		this.$dialog			= $( $template.html() );	$template.remove();  // nb: clone template & remove it from DOM
		this.$blockingDiv		= self.$dialog.find( '.modal-content' );
		this.metadataDict		= ko.observable( null );
		this.metadataInitial	= ko.observable( null );
		this.data				= ko.observable( null );
		this.dataInitial		= ko.observable( null );

		this.metadata = ko.pureComputed({
				read: ()=>
					{
						const dict = self.metadataDict();
						if( dict == null )
							return null;
						return jsonStringify( dict, /*indented*/true );
					},
				write: (value)=>
					{
						try
						{
							if( common.utils.stringIsNullOrWhitespace(value) )
							{
								self.metadataDict( null );
								return;
							}
							const dict = jsonParse( value );
							const json = jsonStringify( dict );
							if( json[0] != '{' )
								// Not a dictionary
								return;
							// Ok
							self.metadataDict( dict );
						}
						catch
						{
							// Invalid => NOOP => leave as is
						}
						finally
						{
							// Hack: force refresh 'self.metadata()' ...
							const a = self.metadataDict();
							self.metadataDict( {} );
							self.metadataDict( null );
							self.metadataDict( a );
						}
					},
			})
		this.metadataChanged = ko.pureComputed( ()=>self.metadataInitial() != jsonStringify(self.metadataDict()) );
		this.dataChanged = ko.pureComputed( ()=>self.dataInitial() != self.data() );

		// Watch for dialog close
		self.$dialog.on( 'hidden.bs.modal', ()=>
			{
				common.utils.log( `NodeDetailsDialog: Close` );  // nb: Closed manually or programmatically
			} );

		ko.applyBindings( self, self.$dialog[0] );
	}

	public async show(node:Node) : Promise<void>
	{
		common.utils.log( `NodeDetailsDialog: show()` );
		const self = this;

		self.node = node;

		common.utils.log( `NodeDetailsDialog: Show modal diaog` );
		self.$dialog.modal();

		await self.refresh();
	}

	private async refresh() : Promise<void>
	{
		common.utils.log( `NodeDetailsDialog: refresh()` );
		const self = this;

		common.html.block( self.$blockingDiv );
		try
		{
			common.utils.log( `NodeDetailsDialog: Retreive metadata & data` );
			const [metadata, data] = await Promise.all([
					ctrl.getNodeMetaData( self.node.path ),
					ctrl.getNodeData( self.node.path ),
				]);
			self.metadataDict( metadata );
			self.metadataInitial( jsonStringify(metadata) );
			self.data( data );
			self.dataInitial( data );
		}
		finally
		{
			common.html.unblock( self.$blockingDiv );
		}
	}

	protected async saveMetadata() : Promise<void>
	{
		common.utils.log( `NodeDetailsDialog: saveMetadata()` );
		const self = this;

		common.html.block( self.$blockingDiv );
		try
		{
			await ctrl.setNodeMetaData( self.node.path, self.metadataDict() );
			await self.refresh();
		}
		finally
		{
			common.html.unblock( self.$blockingDiv );
		}
	}

	protected async saveData() : Promise<void>
	{
		common.utils.log( `NodeDetailsDialog: saveData()` );
		const self = this;

		common.html.block( self.$blockingDiv );
		try
		{
			await ctrl.setNodeData( self.node.path, self.data() );
			await self.refresh();
		}
		finally
		{
			common.html.unblock( self.$blockingDiv );
		}
	}
}

class NodeUploadDialog
{
	private readonly	$dialog				: JQuery;
	private readonly	$blockingDiv		: JQuery;
	private				node				: Node;
	protected readonly	selectedFiles		: KnockoutObservable<FileList>;
	protected readonly	selectedFile		: KnockoutComputed<File>;
	protected readonly	uploadToChild		: KnockoutObservable<boolean>;
	protected readonly	childName			: KnockoutObservable<string>;
	protected readonly	overwriteChild		: KnockoutObservable<boolean>;
	protected readonly	canUpload			: KnockoutComputed<boolean>;
	protected readonly	destinationPath		: KnockoutComputed<string>;

	constructor($template:JQuery)
	{
		common.utils.log( `NodeUploadDialog: constructor` );
		const self = this;
		this.$dialog			= $( $template.html() );	$template.remove();  // nb: clone template & remove it from DOM
		this.$blockingDiv		= self.$dialog.find( '.modal-content' );
		this.selectedFiles		= ko.observable( null );
		this.uploadToChild		= ko.observable( false );
		this.childName			= ko.observable( '' );
		this.overwriteChild		= ko.observable( false );

		this.selectedFile = ko.pureComputed( ()=>
			{
				const files = self.selectedFiles();
				if( (files == null) || (files.length == 0) )
					return null;
				return files[0];
			} );

		self.childName.subscribe( ()=>self.checkChildName() );

		this.canUpload = ko.pureComputed( ()=>
			{
				if( self.selectedFile() == null )
					return false;
				if( self.uploadToChild() == false )
					return true;
				if( common.utils.stringIsNullOrWhitespace(self.childName()) )
					return false;
				return true;
			} );

		this.destinationPath = ko.pureComputed( ()=>
			{
				if( ! self.canUpload() )
					return null;
				if( ! self.uploadToChild() )
					return self.node.path;
				return `${self.node.path}${ctrl.pathSeparator}${self.childName()}`;
			} );

		// Watch for dialog close
		self.$dialog.on( 'hidden.bs.modal', ()=>
			{
				common.utils.log( `NodeUploadDialog: Close` );  // nb: Closed manually or programmatically
			} );

		ko.applyBindings( self, self.$dialog[0] );
	}

	public async show(node:Node) : Promise<void>
	{
		common.utils.log( `NodeUploadDialog: show()` );
		const self = this;

		self.uploadToChild( false );
		self.childName( '' );
		self.overwriteChild( false );
		self.node = node;

		common.utils.log( `NodeUploadDialog: Show modal diaog` );
		self.$dialog.modal();
	}

	private async checkChildName() : Promise<void>
	{
		common.utils.log( `NodeUploadDialog: checkChildName()` );
		const self = this;

		common.html.block( self.$blockingDiv );
		try
		{
			const orig = self.childName();
			if( common.utils.stringIsNullOrEmpty(orig) )
				// NOOP
				return;
			const sanitized = await ctrl.sanitizeName( orig );

			if( self.childName() != orig )
				// Value changed before the response received => Discard
				return;
			if( sanitized != orig )
				// Must be corrected
				self.childName( sanitized );
		}
		finally
		{
			common.html.unblock( self.$blockingDiv );
		}
	}

	protected fileChanged(a:any, b:any, c:any) : void
	{
		console.log({ a,b,c })
	}

	protected async upload() : Promise<void>
	{
		common.utils.log( `NodeUploadDialog.upload()` );
		const self = this;

		common.html.block( $blockingDiv );
		try
		{
			const file = self.selectedFile();
			const path = self.destinationPath();
			const overwrite = self.uploadToChild() ? self.overwriteChild() : true;

			common.utils.log( `NodeUploadDialog.upload(): Upload file`, file );
			const rc = await ctrl.tempUpload( file );
			if(! rc.success )
			{
				common.utils.error( 'File upload error', { rc } );
				common.html.showMessage( rc.errorMessage );
				return;
			}
			const tempFileID = rc.fileID;

			common.utils.log( `NodeUploadDialog.upload(): Start restore at '${path}'` );
			await ctrl.restoreTree({ path, tempFileID, overwrite });

			common.utils.log( `NodeUploadDialog.upload(): Close dialog` );
			self.$dialog.modal( 'hide' );

			common.utils.log( `NodeUploadDialog.upload(): Refresh node` );
			await self.node.refresh( /*recursive*/true );
		}
		catch( err )
		{
			common.utils.error( `upload() threw an error`, err );
			common.html.showMessage( ''+err );
		}
		finally
		{
			common.html.unblock( $blockingDiv );
		}
		common.utils.log( `NodeUploadDialog.upload(): END` );
	}
}
