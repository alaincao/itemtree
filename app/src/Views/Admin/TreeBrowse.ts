import { jsonStringify } from "../utils";
import * as common from "../common";
import { getChildNodes, getNodeMetaData, getPathSegments, pathSeparator } from "../../Tree/TreeController";

var $blockingDiv : JQuery;
export var rootNode : Node;
const startFolded = true;

export async function init(p:{
								$blockingDiv	: JQuery,
							}) : Promise<void>
{
	$blockingDiv	= p.$blockingDiv;

	rootNode = new Node( pathSeparator );
	rootNode.isFolded( false );
}

class Node
{
	public readonly	path		: string;
	public readonly	segments	: string[];
	public readonly	name		: string;
	public readonly	children	: KnockoutObservableArray<Node>;
	public readonly	hasChildren	: KnockoutObservable<boolean>;
	public readonly	isFolded	: KnockoutObservable<boolean>;

	constructor(path:string)
	{
		const self = this;
		this.path		= path;
		this.segments	= getPathSegments( path );
		this.name		= (self.segments.length == 0) ? /*root*/pathSeparator : self.segments[ self.segments.length-1 ];
		this.children	= ko.observableArray([]);
		this.isFolded	= ko.observable( startFolded );

		this.hasChildren = ko.pureComputed( ()=>self.children().length > 0 );
		self.isFolded.subscribe( ()=>
			{
				if( self.isFolded() )
					self.children( [] );
				else
					self.refresh();
			} );
	}

	public toggleFolded() : void
	{
		this.isFolded( ! this.isFolded() );
	}

	public async refresh() : Promise<void>
	{
		const self = this;

		if( self.isFolded() )
		{
			// Just ensure the children list is empty
			self.children( [] );
			return;
		}

		// Retreive children
		common.html.block( $blockingDiv );
		try
		{
			const origs : {[key:string]:Node} = {};
			for( const node of self.children() )
				origs[ node.name ] = node;

			const childNodes = await getChildNodes( self.path );
			const news = childNodes.map( childName=>
				{
					let childNode = origs[ childName ];
					if( childNode == null )
						// Create new
						childNode = new Node( pathSeparator+self.segments.concat(childName).join(pathSeparator) );
					return childNode
				} );
			self.children( news );

			// Refresh recursive
			const tasks = news.map( node=>node.refresh() );
			await Promise.all( tasks );
		}
		finally
		{
			common.html.unblock( $blockingDiv );
		}
	}

	public async showMetadata() : Promise<void>
	{
		const self = this;
		const data = await getNodeMetaData( self.path );
		common.html.showHtml( `<pre>${jsonStringify(data)}</pre>` );
	}
}
