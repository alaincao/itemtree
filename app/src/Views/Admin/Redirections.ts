
import { jsonParse } from "../utils";
import * as common from "../common";
import * as ctrl from "../../Tree/TreeController";
import * as base from "../../Services/Redirections";

const saveSucceedMessage : string = 'Redirections saved';

export var redirections : KnockoutObservableArray<EntryKO>;
var $blockingDiv : JQuery;

export async function init(p:{
								$blockingDiv	: JQuery,
							}) : Promise<void>
{
	redirections	= ko.observableArray( [] );
	$blockingDiv	= p.$blockingDiv;
	await refresh();
}

export async function refresh() : Promise<void>
{
	common.utils.log(  `Retreive redirections` );
	const json = await ctrl.getNodeData( common.routes.api.redirections );
	const entries = <base.Entry[]>jsonParse( json??'[]' );

	common.utils.log(  `Create list` );
	redirections( entries.map( entry=>new EntryKO(entry) ) );
}

export function toModel() : base.Entry[]
{
	const list = redirections()
					.map( v=>v.toModel() )
					.filter( v=>! common.utils.stringIsNullOrWhitespace(v.source) )
					.filter( v=>! common.utils.stringIsNullOrWhitespace(v.destination) );
	list.sort( (a,b)=> (a.source < b.source) ? -1
					 : (a.source > b.source) ? 1
					 : 0 );
	return list;
}

export function addNew() : void
{
	redirections.push( new EntryKO() );
}

export async function save() : Promise<void>
{

	common.html.block( $blockingDiv );
	try
	{
		common.utils.log(  `Save` );
		const entries = toModel();
		const rc = await base.save( entries );
		if(! rc.success )
		{
			common.utils.error( 'Save error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		common.utils.log(  `Refresh` );
		await refresh();

		common.html.showMessage( saveSucceedMessage );
	}
	finally
	{
		common.html.unblock( $blockingDiv );
	}
}

class EntryKO extends base.EntryKO
{
	public clear() : void
	{
		const self = this;
		self.source( '' );
		self.destination( '' );
	}
}
