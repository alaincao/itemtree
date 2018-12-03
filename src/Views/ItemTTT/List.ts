
import * as common from "../common";
import { list } from "./ItemTTTController";

function sleep(ms:number) : Promise<void>
{
	return new Promise( resolve => setTimeout(resolve, ms) );
}
// DELETEME
async function testPromise() : Promise<void>
{
	common.utils.log( 'A', `${new Date()}` );
	var task = sleep( 3000 );
	common.utils.log( 'B', `${new Date()}` );
	await task;
	common.utils.log( 'C', `${new Date()}` );
}

var $divCarsList : JQuery;

export async function init(p:{	$divCarsList	: JQuery,
								$btnViewList	: JQuery,
								$btnViewGrid	: JQuery,
								$btnSortName	: JQuery,
								$btnSortPrice	: JQuery }) : Promise<void>
{
	common.utils.log( 'list.init() START' );
	testPromise();

	$divCarsList = p.$divCarsList;
	p.$btnViewGrid	.click( ()=>{ refreshList({ viewMode:list.ViewModes.grid }) } );
	p.$btnViewList	.click( ()=>{ refreshList({ viewMode:list.ViewModes.list }) } );
	p.$btnSortName	.click( ()=>{ refreshList({ sortingField:list.SortingFields.name }) } );
	p.$btnSortPrice	.click( ()=>{ refreshList({ sortingField:list.SortingFields.price }) } );

	common.utils.log( 'list.init() END' );
}

async function refreshList(p:list.GetListContentRequest) : Promise<void>
{
	common.utils.log( 'list.refresh() START' );

	p.noLayout = true;
	var html = await list.getListContent( p );
	$divCarsList.html( html );

	common.utils.log( 'list.refresh() END' );
}

