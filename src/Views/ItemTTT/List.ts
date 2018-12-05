
import * as common from "../common";
import { list } from "./ItemTTTController";

var currentSortingField : list.SortingField;
var $divCarsList : JQuery;

export async function init(p:{	sortingField	: list.SortingField,
								$divCarsList	: JQuery,
								$btnViewList	: JQuery,
								$btnViewGrid	: JQuery,
								$btnSortName	: JQuery,
								$btnSortPrice	: JQuery }) : Promise<void>
{
	common.utils.log( 'list.init() START' );

	currentSortingField	= p.sortingField;
	$divCarsList		= p.$divCarsList;
	p.$btnViewGrid	.click( ()=>{ refreshList({ viewMode:list.ViewModes.grid }) } );
	p.$btnViewList	.click( ()=>{ refreshList({ viewMode:list.ViewModes.list }) } );
	p.$btnSortName	.click( ()=>{ refreshList({ sortingField:list.SortingFields.name }) } );
	p.$btnSortPrice	.click( ()=>{ refreshList({ sortingField:list.SortingFields.price }) } );

	common.utils.log( 'list.init() END' );
}

async function refreshList(p:list.GetListContentRequest) : Promise<void>
{
	common.utils.log( 'list.refreshList() START', { p, currentSortingField } );

	// Ask for the stripped version of the page (no layout)
	p.noLayout = true;

	// Switch to descending order if required
	if( p.sortingField != null )
	{
		if( p.sortingField == currentSortingField )
		{
			switch( currentSortingField )
			{
				case list.SortingFields.name:	p.sortingField = list.SortingFields.name_desc;	break;
				case list.SortingFields.price:	p.sortingField = list.SortingFields.price_desc;	break;
			}
		}

		currentSortingField = p.sortingField;
	}

	// GET and replace list DOM
	var html = await list.getListContent( p );
	$divCarsList.html( html );

	common.utils.log( 'list.refresh() END', { currentSortingField } );
}

