
import * as common from "../Views/common";
import { BaseAutoItem, DictObj } from "../Utils/BaseAutoItem";
export { DictObj, DictKO } from "../Utils/BaseAutoItem";
import { ItemPicture } from "./ItemPicture";

export interface Item extends DictObj
{
	pictures : ItemPicture[];
}

export class ItemKO extends BaseAutoItem
{
	public readonly code		: KnockoutObservable<string>;
	public readonly name		: KnockoutObservable<string>;
	public readonly price		: KnockoutObservable<number>;
	public readonly pictures	: KnockoutObservableArray<ItemPicture>;

	constructor($container:JQuery, src?:Item)
	{
		if( src == null )
			src = { name:'', pictures:[] };
		super( $container, src );
		const self = this;
		this.pictures = ko.observableArray( src.pictures );

		if( self.price != null )
			common.utils.ensureInteger({ observable:self.price, canBeZero:false, canBeNull:true, mustBePositive:true });
	}

	public /*override*/ toDictObj(dict?:DictObj) : Item
	{
		const rc = super.toDictObj( dict );
		return <Item>rc;
	}

	public /*override*/ fromDictObj(item?:Item) : void
	{
		super.fromDictObj( item );
		this.pictures( item.pictures );
	}
}
