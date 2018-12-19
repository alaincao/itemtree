
import * as common from "../common";
import * as dto from "../../DTOs/Item";
import { ItemPicture } from "../../DTOs/ItemPicture";
import * as ctrl from "../../Services/ItemController";
import * as picCtrl from "../../Services/ItemPictureController";
import * as trnCtrl from "../../Services/TranslationController";
import { TranslationKO } from "../../DTOs/Translation";

const message_confirmDelete = 'Are you sure you want to delete that item?';
const message_saveSuccess = 'Item saved successfully';
const message_refreshFailed = 'An error occured while refresing the data: ';
const message_confirmDeletePicture = 'Are you sure you want to delete that image?';

var $blockingDiv : JQuery;
var originalCode : string;

export var item			: ItemKO;
export var picDropZone	: { hover:KnockoutObservable<boolean> };

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
	item = new ItemKO( p.$fieldsContainer, p.model );
	originalCode = item.code();

	common.utils.log( 'edit.init(): Bind JQuery events' );
	p.$btnSave.click( save );
	p.$btnDelete.click( ()=>{ delete_(); } );

	common.utils.log( 'edit.init(): Init picture upload' );
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
}

export async function autoCompleteFeature(searchTerm:string) : Promise<string[]>
{
	var rc = await trnCtrl.autoCompleteResolve({ type:trnCtrl.TranslationTypes.feature, searchString:searchTerm, includeTranslations:true });
	if(! rc.success )
	{
		common.utils.error( 'autocmplete error', { rc } );
		return [];
	}
	return rc.list;
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

async function delete_(confirmed?:boolean) : Promise<void>
{
	common.utils.log( 'edit.delete(): START' );

	if( confirmed != true )
	{
		common.utils.log( 'edit.delete(): Ask confirmation' );
		const rc = await common.html.confirm( message_confirmDelete );
		if(! rc )
		{
			common.utils.log( 'edit.delete(): NOT CONFIRMED' );
			return;
		}
	}

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

	await refresh();

	common.utils.log( 'edit.uploadPicture(): END' );
}

export async function deletePicture(picture:ItemPicture, confirmed?:boolean) : Promise<void>
{
	common.utils.log( 'edit.deletePicture(): START' );

	if( confirmed != true )
	{
		common.utils.log( 'edit.deletePicture(): Ask confirmation' );
		const rc = await common.html.confirm( message_confirmDeletePicture );
		if(! rc )
		{
			common.utils.log( 'edit.deletePicture(): NOT CONFIRMED' );
			return;
		}
	}

	common.utils.log( 'edit.deletePicture(): Launch delete request' );
	common.html.block( $blockingDiv );
	const rc = await picCtrl.delete_({ itemCode:picture.itemCode, number:picture.number });
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.deletePicture(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
	}

	common.utils.log( 'edit.deletePicture(): Launch refresh' );
	await refresh();

	common.utils.log( 'edit.deletePicture(): END' );
}

export async function toggleMainPicture(picture:ItemPicture) : Promise<void>
{
	common.utils.log( 'edit.toggleMainPicture(): START' );

	common.utils.log( 'edit.toggleMainPicture(): Launch toggle request' );
	common.html.block( $blockingDiv );
	const rc = await picCtrl.setMain({ itemCode:picture.itemCode, number:picture.number, isMain:(!picture.isMainImage) });
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.toggleMainPicture(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
	}

	common.utils.log( 'edit.toggleMainPicture(): Launch refresh' );
	await refresh();

	common.utils.log( 'edit.toggleMainPicture(): END' );
}

export async function reorderPicture(picture:ItemPicture, offset:number) : Promise<void>
{
	common.utils.log( 'edit.movePicture(): START' );

	let newNumber = picture.number + offset;
	if( newNumber <= 0 )
		newNumber = 1;
	if( picture.number == newNumber )
		// NOOP
		return;

	common.utils.log( 'edit.movePicture(): Launch move request' );
	common.html.block( $blockingDiv );
	const rc = await picCtrl.reorder({ itemCode:picture.itemCode, number:picture.number, newNumber:newNumber });
	common.html.unblock( $blockingDiv );

	if(! rc.success )
	{
		common.utils.error( 'edit.movePicture(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
	}

	common.utils.log( 'edit.movePicture(): Launch refresh' );
	await refresh();

	common.utils.log( 'edit.movePicture(): END' );
}

async function refresh(code?:string) : Promise<boolean>
{
	// TODO: Allow refresh only images to avoid overwriting any modifications to input fields ...
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

/** Used to add a changing parameter to the URL to bypass browser's cache */
export function scrambleUrl(url:string) : string
{
	if( url.indexOf('?') >= 0 )
		url = url + '&';
	else
		url = url + '?';
	url = url + 'p=' + common.utils.newGuid();
	return url;
}

class ItemKO extends dto.ItemKO
{
	public readonly features		: KnockoutObservableArray<TranslationKO>;
	public readonly pictures		: KnockoutObservableArray<ItemPicture>;

	constructor($container:JQuery, src:dto.Item)
	{
		super( $container, src );
		this.features	= ko.observableArray( src.features.map(v=>new TranslationKO(v)) );
		this.pictures	= ko.observableArray( src.pictures );
	}

	protected addNewFeature() : void
	{
		const self = this;
		self.features.push( new TranslationKO() );
	}

	public /*override*/ toDictObj(dict?:dto.DictObj) : dto.Item
	{
		const self = this;
		const item = <dto.Item>super.toDictObj( dict );
		item.features	= self.features().map( v=>{ return { en:v.en(), fr:'', nl:'' }; } );  // nb: only EN is used server-side
		return item;
	}

	public /*override*/ fromDictObj(item:dto.Item) : void
	{
		const self = this;
		super.fromDictObj( item );
		self.features( item.features.map( v=>{ return new TranslationKO(v); } ) );
		self.pictures( item.pictures );
	}
}
