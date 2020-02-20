
import * as common from "../common";
import { addKoTinymceEditor } from "../Blog/Edit";
import * as ctrl from "../../Services/DynamicPageController";

var $blockingDiv	: JQuery;
var itemCode		: string;
export var textHtml	: KnockoutObservable<string>;

export async function init(p:{	$blockingDiv	: JQuery,
								itemCode		: string,
							}) : Promise<void>
{
	common.utils.log( 'show.init() START', { p } );

	$blockingDiv	= p.$blockingDiv;
	itemCode		= p.itemCode;
	textHtml		= ko.observable();

	common.utils.log( 'show.init(): add custom Knockout bindings handler' );
	addKoTinymceEditor();

	common.utils.log( 'show.init() END' );
}

export async function save() : Promise<void>
{
	common.utils.log( 'show.save(): START' );
	common.html.block( $blockingDiv );

	common.utils.log( 'show.save(): Send the save request' );
	await ctrl.update({ itemCode, text:textHtml() });

	common.utils.log( 'show.save(): Refresh' );
	await refresh();

	common.html.unblock( $blockingDiv );
	common.utils.log( 'show.save(): END' );
}

export async function refresh() : Promise<void>
{
	common.utils.log( 'show.refresh(): START' );
	common.html.block( $blockingDiv );

	common.utils.log( 'show.save(): Send the get request' );
	const newHtml = await ctrl.get({ itemCode });

	common.utils.log( 'show.save(): Update html' );
	textHtml( newHtml );

	common.html.unblock( $blockingDiv );
	common.utils.log( 'show.refresh(): END' );
}
