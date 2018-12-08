
import * as common from "../common";
import * as dto from "../../DTOs/Item";
import * as ctrl from "../../Services/ItemController";

const message_saveSuccess = 'Item saved successfully';
const message_refreshFailed = 'An error occured while refresing the data: ';

var $blockingDiv : JQuery;
var originalCode : string;
export var item : dto.ItemKO;

export async function init(p:{	model				: dto.Item,
								$blockingDiv		: JQuery,
								$btnSave			: JQuery,
								$fieldsContainer	: JQuery }) : Promise<void>
{
	common.utils.log( 'edit.init() START', { p } );
	$blockingDiv = p.$blockingDiv;

	common.utils.log( 'edit.init(): Create KO item' );
	item = new dto.ItemKO( p.$fieldsContainer, p.model );
	originalCode = item.code();

	common.utils.log( 'edit.init(): Apply KO item' );
	ko.applyBindings( item, p.$fieldsContainer[0] );

	common.utils.log( 'edit.init(): Bind JQuery events' );
	p.$btnSave.click( save );

	common.utils.log( 'edit.init() END' );
}

async function save() : Promise<void>
{
	common.utils.log( 'edit.save(): START' );

	const obj = item.toDictObj();

	common.utils.log( 'edit.save(): Launch save' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.save({ originalCode, item:obj });
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.save(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}

	common.utils.log( 'edit.save(): Refresh values', { rc } );
	const rcr = await refresh( rc.newCode );
	if( rcr )
		common.html.showMessage( message_saveSuccess );

	common.utils.log( 'edit.save(): END' );
}

async function refresh(code?:string) : Promise<boolean>
{
	common.utils.log( 'edit.refresh(): START' );

	let newUrl : string = null;
	if( code == null )
	{
		code = originalCode;
	}
	else
	{
		if( originalCode != code )
			// URL must change!
			newUrl = common.routes.itemTTT.itemEdit.replace( common.routes.itemCodeParameter, code );
		originalCode = code;
	}

	common.utils.log( 'edit.refresh(): Launch request' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.details( code );
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.refresh(): Error', { rc } );
		common.html.showMessage( message_refreshFailed+rc.errorMessage );
		return false;
	}

	common.utils.log( 'edit.refresh(): Load from new item', { rc } );
	item.fromDictObj( rc.item );

	if( newUrl != null )
	{
		common.utils.log( `edit.refresh(): Update URL to "${newUrl}"` );
		common.url.pushHistory({ pathname:newUrl, newTitle:item.name() });
	}

	common.utils.log( 'edit.refresh(): END' );
	return true;
}
