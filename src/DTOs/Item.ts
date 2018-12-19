
import * as common from "../Views/common";
import { BaseAutoItem, DictObj } from "../Utils/BaseAutoItem";
export { DictObj, DictKO } from "../Utils/BaseAutoItem";
import { Translation } from "./Translation";
import { ItemPicture } from "./ItemPicture";

export interface Item extends DictObj
{
	code		: string;
	active		: boolean;
	features	: Translation[];
	pictures	: ItemPicture[];
}

export class ItemKO extends BaseAutoItem
{
	public readonly code			: KnockoutObservable<string>;
	public readonly name			: KnockoutObservable<string>;
	public readonly descriptionEN	: KnockoutObservable<string>;
	public readonly descriptionFR	: KnockoutObservable<string>;
	public readonly descriptionNL	: KnockoutObservable<string>;
	public readonly active			: KnockoutObservable<boolean>;
	public readonly price			: KnockoutObservable<number>;

	constructor($container:JQuery, src?:Item, fieldNames?:string[])
	{
		if( src == null )
			src = { code:null, active:false, features:[], pictures:[] };
		super( $container, src, fieldNames );
		const self = this;

		if( self.price != null )
			common.utils.ensureInteger({ observable:self.price, canBeZero:false, canBeNull:true, mustBePositive:true });
	}

	public /*override*/ toDictObj(dict?:DictObj) : Item
	{
		const rc = super.toDictObj( dict );
		return <Item>rc;
	}
}
