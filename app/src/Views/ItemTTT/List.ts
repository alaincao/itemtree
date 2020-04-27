
import { nameof, stringIsNullOrWhitespace } from "../utils";
import * as common from "../common";
import * as itemCtrl from "../../Services/ItemController";
import { list } from "./ItemTTTController";
import * as dto from "../../DTOs/Item";
import { Languages } from "../../Language";

const modelSearchString = '[type=ttt-model]';

var currentSortingField : list.SortingField;
var $txtSearch		: JQuery;
var $cbShowInactive	: JQuery;
var $divCarsList	: JQuery;
var $divBlocking	: JQuery;

export var itemsList : ItemKO[];

export async function init(p:{	sortingField	: list.SortingField,
								$txtSearch		: JQuery,
								$cbShowInactive	: JQuery,
								$divCarsList	: JQuery,
								$btnViewList	: JQuery,
								$btnViewGrid	: JQuery,
								$btnSortName	: JQuery,
								$btnSortPrice	: JQuery }) : Promise<void>
{
	common.utils.log( 'list.init(): START' );

	currentSortingField	= p.sortingField;
	$txtSearch			= p.$txtSearch;
	$cbShowInactive		= p.$cbShowInactive.prop( 'checked', true );
	$divCarsList		= p.$divCarsList;
	$divBlocking		= $divCarsList;
	p.$btnViewGrid	.click( ()=>{ refreshList({ viewMode:list.ViewModes.grid }) } );
	p.$btnViewList	.click( ()=>{ refreshList({ viewMode:list.ViewModes.list }) } );
	p.$btnSortName	.click( ()=>{ refreshList({ sortingField:list.SortingFields.name }) } );
	p.$btnSortPrice	.click( ()=>{ refreshList({ sortingField:list.SortingFields.price }) } );

	reconstructItemsList();

	common.utils.log( 'list.init(): Bind JQuery events' );
	$txtSearch.keyup( ()=>{ filterItemsList(); } );
	$cbShowInactive.change( ()=>{ filterItemsList(); } );

	common.utils.log( 'list.init(): END' );
}

function reconstructItemsList() : void
{
	common.utils.log( 'list.reconstructItemsList(): START' );

	const $models = $divCarsList.find( modelSearchString );

	common.utils.log( `list.reconstructItemsList(): Reconstructing ${$models.length} models`  );
	const items : ItemKO[] = [];
	$models.each( (i,e)=>
		{
			// Find the JSON model's tag & set its parent element as the item's container
			const $modelElement = $(e);
			const $container = $modelElement.parent();
			const model = <dto.Item>JSON.parse( $modelElement.text() );

			// Remove the model tag (i.e. free memory ?)
			$modelElement.remove();

			// Create the ItemKO and add it to the list
			const item = new ItemKO( $container, model );
			items.push( item );
		} );

	common.utils.log( 'list.reconstructItemsList(): Replacing itemsList'  );
	itemsList = items;

	common.utils.log( 'list.reconstructItemsList(): END' );
}

export function filterItemsList() : void
{
	itemsList.forEach( (item)=>
		{
			item.visible( item.getIsVisible() );
		} );
}

async function refreshList(p:list.GetListContentRequest) : Promise<void>
{
	common.utils.log( 'list.refreshList(): START', { p, currentSortingField } );

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

	common.utils.log( 'list.refreshList(): Request html & replace list DOM' );
	common.html.block( $divBlocking );
	var html = await list.getListContent( p );
	common.html.unblock( $divBlocking );
	$divCarsList.html( html );

	reconstructItemsList();

	common.utils.log( 'list.refreshList(): END', { currentSortingField } );
}

class ItemKO extends dto.ItemKO
{
	public readonly $container	: JQuery;
	public readonly visible		: KnockoutObservable<boolean>;

	constructor($container:JQuery, model:dto.Item)
	{
		const required = [	nameof<dto.ItemKO>('code'),
							nameof<dto.ItemKO>('name'),
							nameof<dto.ItemKO>('descriptionEN'),
							nameof<dto.ItemKO>('descriptionFR'),
							nameof<dto.ItemKO>('descriptionNL'),
							nameof<dto.ItemKO>('active') ];
		super( $container, model, required );
		const self = this;
		this.$container	= $container;
		this.visible	= ko.observable( self.getIsVisible() );

		// Bind this object to its container
		ko.applyBindings( self, $container[0] );
	}

	public async toggleActive() : Promise<void>
	{
		common.utils.log( 'list.ItemKO.toggleActive(): START' );
		const self = this;

		common.utils.log( 'list.ItemKO.toggleActive(): Retreive original model' );
		common.html.block( $divBlocking );
		{
			const rc = await itemCtrl.details( self.code() );
			if(! rc.success )
			{
				common.html.unblock( $divBlocking );
				common.utils.error( 'Item retreive error', { rc } );
				return;
			}
			var model = rc.item;
		}

		common.utils.log( 'list.ItemKO.toggleActive(): Toggle active flag & save' );
		{
			model.active = (! model.active);
			const rc = await itemCtrl.save({ originalCode:model.code, item:model });
			if(! rc.success )
				common.utils.error( 'Item save error', { rc } );
		}

		common.utils.log( 'list.ItemKO.toggleActive(): Refresh' );
		await refreshList({});

		common.html.unblock( $divBlocking );
		common.utils.log( 'list.ItemKO.toggleActive(): END' );
	}

	public getIsVisible() : boolean
	{
		const self = this;

		// Check "show active" checkbox

		const showInactive = $cbShowInactive.is( ':checked' );
		if( ! self.active() )
			if( ! showInactive )
				return false;

		// Check "search" textbox

		let searchString = <string>$txtSearch.val();
		searchString = (searchString == null ? '' : searchString).toLowerCase().trim();
		if( searchString == '' )
			return true;
		let searchWords : string[] = [];
		$.each( searchString.split(' '), function(i,word)
			{
				if( stringIsNullOrWhitespace(word) )
					return;
				searchWords.push( word.trim() );
			} );

		const searchFields : string[] = [];
		searchFields.push( self.name() );
		switch( common.pageParameters.currentLanguage )
		{
			case Languages.en:	searchFields.push( self.descriptionEN() );	break;
			case Languages.fr:	searchFields.push( self.descriptionFR() );	break;
			case Languages.nl:	searchFields.push( self.descriptionNL() );	break;
		}

		let matchCount = 0;
		searchWords.forEach( (word,i)=>
			{
				for( let i=0; i<searchFields.length; ++i )
				{
					const field = searchFields[i].toLowerCase();
					if( field.indexOf(word) >= 0 )
					{
						++ matchCount;
						return;
					}
				}
			} );
		if( matchCount != searchWords.length )  // All words must match
			return false;

		return true;
	}
}
