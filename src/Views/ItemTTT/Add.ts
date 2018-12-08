
import * as common from "../common";
import * as dto from "../../DTOs/Item";
import * as ctrl from "../../Services/ItemController";

var $blockingDiv : JQuery;
export var item : dto.ItemKO;

export async function init(p:{	$blockingDiv		: JQuery,
								$btnSave			: JQuery,
								$fieldsContainer	: JQuery }) : Promise<void>
{
	common.utils.log( 'add.init() START', { p } );
	$blockingDiv = p.$blockingDiv;

	common.utils.log( 'add.init(): Create KO item' );
	item = new dto.ItemKO( p.$fieldsContainer );

	common.utils.log( 'add.init(): Apply KO item' );
	ko.applyBindings( item, p.$fieldsContainer[0] );

	common.utils.log( 'add.init(): Bind JQuery events' );
	p.$btnSave.click( save );

	common.utils.log( 'add.init() END' );
}

async function save() : Promise<void>
{
	common.utils.log( 'add.save(): START' );

	const obj = item.toDictObj();

	common.utils.log( 'add.save(): Launch save' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.save({ item:obj });
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'add.save(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}

	common.utils.log( 'add.save(): Redirect', { rc } );
	const url = common.routes.itemTTT.itemEdit.replace( common.routes.itemCodeParameter, rc.newCode );
	common.utils.log( `add.save(): url="${url}"` );
	common.url.redirect( url );

	common.utils.log( 'add.save(): END' );
}
