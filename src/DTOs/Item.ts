
import * as common from "../Views/common";
import { BaseAutoItem, DictObj } from "../Utils/BaseAutoItem";
export { DictObj, DictKO } from "../Utils/BaseAutoItem";

export interface Item extends DictObj
{
}

export class ItemKO extends BaseAutoItem
{
	public readonly code			: KnockoutObservable<string>;
	public readonly name			: KnockoutObservable<string>;
	public readonly price			: KnockoutObservable<number>;

	constructor($container:JQuery, src?:Item)
	{
		if( src == null )
			src = { name:'' };
		super( $container, src );
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
