
import * as common from "../common";
import * as dto from "../../DTOs/Item";
import * as ctrl from "../../Services/ItemController";
import * as picCtrl from "../../Services/ItemPictureController";

const message_saveSuccess = 'Item saved successfully';
const message_refreshFailed = 'An error occured while refresing the data: ';

var $blockingDiv : JQuery;
var originalCode : string;
export var item : dto.ItemKO;

export var picDropZone : { hover:KnockoutObservable<boolean> };

export async function init(p:{	model				: dto.Item,
								$blockingDiv		: JQuery,
								$btnSave			: JQuery,
								$btnDelete			: JQuery,
								$fieldsContainer	: JQuery,
								$picUploadDropZone	: JQuery,
								$picUploadControl	: JQuery,
							}) : Promise<void>
{
	common.utils.log( 'edit.init() START', { p } );
	$blockingDiv = p.$blockingDiv;

	common.utils.log( 'edit.init(): Create KO item' );
	item = new dto.ItemKO( p.$fieldsContainer, p.model );
	originalCode = item.code();

	common.utils.log( 'edit.init(): Apply KO item bindings' );
	ko.applyBindings( item, p.$fieldsContainer[0] );

	common.utils.log( 'edit.init(): Bind JQuery events' );
	p.$btnSave.click( save );
	p.$btnDelete.click( delete_ );

	common.utils.log( 'edit.init(): Initi picture upload' );
	initPictureUpload( p.$picUploadDropZone, p.$picUploadControl );

	common.utils.log( 'edit.init() END' );
}

function initPictureUpload($dropZone:JQuery, $upload:JQuery) : void
{
	common.utils.log( 'edit.initDropZone(): Create drop zone object' );

	picDropZone = {
			hover : ko.observable(false),
		};

	common.utils.log( 'edit.initDropZone(): Watch for drag-drops on "upload image drop zone"' );

	$dropZone.on( 'dragenter', function(e)
		{
			picDropZone.hover( true );
			e.stopPropagation();
			e.preventDefault();
		} );
	$dropZone.on( 'dragover', function(e)
		{
			picDropZone.hover( true );
			e.stopPropagation();
			e.preventDefault();
		} );
	$dropZone.on( 'dragleave', function(e)
		{
			picDropZone.hover( false );
		} );
	$dropZone.on( 'drop', function(e)
		{
			picDropZone.hover( false );
			e.preventDefault();

			var files = (<any>e.originalEvent).dataTransfer.files;
			uploadPicture( files );
		});

	$upload.on( 'change', ()=>
		{
			const files = (<HTMLInputElement>$upload[0]).files;
			uploadPicture( files );
		} );

	common.utils.log( 'edit.initDropZone(): Apply KO bindings' );
	ko.applyBindings( picDropZone, $dropZone[0] );
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

async function delete_() : Promise<void>
{
	common.utils.log( 'edit.delete(): START' );

	common.utils.log( 'edit.delete(): Launch delete' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.delete_( originalCode );
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.delete(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}

	common.utils.log( 'edit.delete(): Redirect' );
	const url = common.routes.itemTTT.itemsList.replace( common.routes.languageParameter, common.pageParameters.currentLanguage );
	common.utils.log( 'edit.delete()', { url } );
	common.url.redirect( url );

	common.utils.log( 'edit.delete(): END' );
}

async function uploadPicture(files:FileList) : Promise<void>
{
	common.utils.log( 'edit.uploadPicture(): START', {files} );

	if( files.length == 0 )
		return;
	if( (<any>window).FormData === undefined )
	{
		common.utils.error( "This browser doesn't support HTML5 file uploads!" );
		return;
	}

	common.utils.log( `edit.uploadPicture(): Create '${files.length}' upload tasks` );
	common.html.block( $blockingDiv );
	const tasks = $.map( files, (file:File)=>
		{
			return picCtrl.upload( originalCode, file );
		} );

	common.utils.log( 'edit.uploadPicture(): Wait for tasks to terminate' );
	const rcs = await Promise.all( tasks );
	common.html.unblock( $blockingDiv );
	common.utils.log( 'edit.uploadPicture(): All tasks terminated' );

	$.each( rcs, (i,rc)=>
		{
			if(! rc )
			{
				common.utils.error( 'edit.delete(): Upload error', { i, rc } );
				common.html.showMessage( rc.errorMessage );
			}
		} );

	common.utils.log( 'edit.uploadPicture(): END' );
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
