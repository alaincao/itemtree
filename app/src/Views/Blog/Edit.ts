
import * as tmce from "tinymce";
import * as common from "../common";
import * as dtos from "../../DTOs/BlogPost";
import * as ctrl from "../../Services/BlogController";

const message_confirmRefresh = 'Are your sure you want to reset the current form ?';
const message_confirmDelete = 'Are your sure you want to delete the current post ?';

var isAdding : boolean;
var $blockingDiv : JQuery;
export var post : dtos.BlogPostKO;

export async function init(p:{	isAdding			: boolean,
								post				: dtos.BlogPost,
								$blockingDiv		: JQuery,
								$picUploadControl	: JQuery,
								$txtDate			: JQuery,
							}) : Promise<void>
{
	common.utils.log( 'edit.init() START', { p } );

	isAdding		= p.isAdding;
	$blockingDiv	= p.$blockingDiv;
	post			= new dtos.BlogPostKO( p.post );

	common.utils.log( 'edit.init(): add custom Knockout bindings handler' );
	addKoTinymceEditor();

	common.utils.log( 'edit.init(): Init picture upload' );
	p.$picUploadControl.on( 'change', ()=>
		{
			const files = (<HTMLInputElement>p.$picUploadControl[0]).files;
			uploadPicture( files );
		} );

	common.utils.log( 'edit.init(): Init date picker' );
	p.$txtDate.datepicker({ dateFormat: 'yy-mm-dd' });

	common.utils.log( 'edit.init() END' );
}

/** Add 'tinymceEditor' Knockout bindings handler */
var koTinymceEditorAdded = false;
export function addKoTinymceEditor() : void
{
	if( koTinymceEditorAdded == true )
		// Already done
		return;
	common.utils.log( 'addKoTinymceEditor()' );
	koTinymceEditorAdded = true;

	// Create HTML editor KO's binding
	ko.bindingHandlers.tinymceEditor =
		{
			init : function(element, valueAccessor, allBindings, viewModel, bindingContext)
				{
					const $element = $(element);
					const observable : KnockoutObservable<string> = valueAccessor();

					// Initial sync.
					$element.html( observable() );

					(<any>$element).tinymce( {
							inline	: true,
							plugins	: 'print preview importcss searchreplace autolink save directionality visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons code',
										// anavailable: powerpaste casechange tinydrive advcode mediaembed checklist tinymcespellchecker a11ychecker formatpainter permanentpen pageembed tinycomments mentions linkchecker advtable
										// Unwanted: autosave(reload confirmation dialog)
							setup	: function(ed:tmce.Editor)
										{
											ed.on( 'change', function()
												{
													// HTML has changed
													observable( $element.html() );
												} );
										},
						} );
				},
			update : function(element, valueAccessor)
				{
					// Observable has changed
					const observable : KnockoutObservable<string> = valueAccessor();
					const newHtml = observable();
					const $element = $(element);
					const origHtml = $element.html();
					if( newHtml != origHtml )
						// HTML has changed
						$element.html( newHtml );
					else
						{/*nb: Don't update HTML when not necessary or we loose the cursor position*/}
				},
		};
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

	 const rc = await ctrl.uploadPicture( files[0] );
	 if(! rc.success )
	 {
		 common.utils.error( 'edit.uploadPicture(): Error', { rc } );
		 common.html.showMessage( rc.errorMessage );
		 return;
	 }

	post.imageHtml( rc.imageTagContent );

	common.utils.log( 'edit.uploadPicture(): END' );
}

export async function refresh(confirmed:boolean) : Promise<void>
{
	common.utils.log( 'edit.refresh(): START' );

	common.utils.log( 'edit.refresh(): Wait for user confirmation' );
	if( confirmed != true )
		confirmed = await common.html.confirm( message_confirmRefresh );
	if(! confirmed )
	{
		common.utils.log( 'edit.refresh(): NOT confirmed ; END' );
		return;
	}

	common.utils.log( 'edit.refresh(): Retreive item' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.list({ id:post.id(), includeImages:true, includeInactives:true });
	common.html.unblock( $blockingDiv );
	if(! rc.success )
	{
		common.utils.error( 'retreive post error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}
	common.utils.log( 'edit.refresh(): Update blog post' );
	post.fromDTO( rc.posts[0] );

	common.utils.log( 'edit.refresh(): END' );
}

export async function save() : Promise<void>
{
	common.utils.log( 'edit.save(): START' );

	common.utils.log( 'edit.save(): Create DTO' );
	const dto = post.toDTO();

	common.utils.log( 'edit.save(): Send the save request' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.save( dto );
	common.html.unblock( $blockingDiv );
	if(! rc.success )
	{
		common.utils.error( 'edit.save(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}

	if( isAdding )
	{
		common.utils.log( 'edit.save(): Adding => Redirect to edit page' );
		const url = common.routes.blog.edit.replace( common.routes.itemIDParameter, ''+rc.id );
		common.url.redirect( url );
	}
	else
	{
		await refresh( /*confirmed*/true );
	}

	common.utils.log( 'edit.save(): END' );
}

export async function delete_() : Promise<void>
{
	common.utils.log( 'edit.delete(): START' );

	common.utils.log( 'edit.delete(): Wait for user confirmation' );
	const confirmed = await common.html.confirm( message_confirmDelete );
	if(! confirmed )
	{
		common.utils.log( 'edit.delete(): NOT confirmed ; END' );
		return;
	}

	common.utils.log( 'edit.delete(): Send the delete request' );
	common.html.block( $blockingDiv );
	const rc = await ctrl.delete_( post.id() );
	common.html.unblock( $blockingDiv );
	if(! rc.success )
	{
		common.utils.error( 'edit.delete(): Error', { rc } );
		common.html.showMessage( rc.errorMessage );
		return;
	}

	common.utils.error( 'edit.delete(): Redirect to list page' );
	const url = common.routes.blog.list.replace( common.routes.languageParameter, common.pageParameters.currentLanguage );
	common.url.redirect( url );

	common.utils.log( 'edit.delete(): END' );
}
