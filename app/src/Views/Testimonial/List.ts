
import { stringIsNullOrWhitespace } from "../utils";
import * as common from "../common";
import * as dtos from "../../DTOs/Testimonial";
import * as ctrl from "../../Services/TestimonialController";

const testimonialIdAttribute = 'ttt-testimonial-id';
var message_imageUploadError : string;
const message_confirmDelete = 'Are you sure you want to delete this testimonial?';

export var formTestimonial : FormTestimonial;
var saveSuccessMessage : string;

export function init(p:{	urlImgNotFound			: string,
							imageUploadErrorMessage	: string,
							saveSuccessMessage		: string,
							$blockingDiv			: JQuery,
							$formDialog				: JQuery,
							$picUploadControl		: JQuery }) : void
{
	common.utils.log( 'list.init(): START' );

	message_imageUploadError = p.imageUploadErrorMessage;
	saveSuccessMessage = p.saveSuccessMessage;

	common.utils.log( 'list.init(): Create KO for new testimonial form' );
	formTestimonial = new FormTestimonial( p.$formDialog, p.$blockingDiv, p.urlImgNotFound );

	common.utils.log( 'list.init(): Init picture upload' );
	p.$picUploadControl.click( ()=>
		{
			// Clear upload control so selecting the same image twice triggers the 'change' event
			p.$picUploadControl.val( null );
		} );
	p.$picUploadControl.change( ()=>
		{
			const files = (<HTMLInputElement>p.$picUploadControl[0]).files;
			/*await*/ formTestimonial.uploadPicture( files );
		} );

	if( common.pageParameters.isAutenticated )  // Nb: binding actions on testimonial entries is required only for the administrator
	{
		common.utils.log( 'list.init(): Bind all testimonial items' );
		$(`[${testimonialIdAttribute}]`).each( (i,e)=>
			{
				const $e = $(e);
				const id = parseInt( $e.attr(testimonialIdAttribute) );
				const o = new TestimonialItem( id, $e, p.$blockingDiv );
				ko.applyBindings( o, $e[0] );
			} );
	}

	common.utils.log( 'list.init(): END' );
}

export class TestimonialItem
{
	private readonly	id				: number;
	private readonly	$blockingDiv	: JQuery;

	constructor(id:number, $container:JQuery, $blockingDiv:JQuery)
	{
		common.utils.log( `TestimonialItem(): id:${id}` );
		this.id				= id;
		this.$blockingDiv	= $blockingDiv;
	}

	protected async toggleActive() : Promise<void>
	{
		const self = this;
		common.utils.log( `TestimonialItem.toggleActive(): START: id:'${self.id}'` );

		common.html.block( self.$blockingDiv );
		const rc1 = await ctrl.list({ id:self.id, includeImages:false, includeInactives:true });
		if(! rc1 )
		{
			common.html.unblock( self.$blockingDiv );
			common.utils.error( 'retreive testimonial error', { rc1 } );
			common.html.showMessage( rc1.errorMessage );
			return;	
		}
		if( rc1.testimonials.length != 1 )
		{
			common.html.showMessage( 'Error while retreiving testimonial' );
			return;
		}
		const dto = rc1.testimonials[0];

		common.utils.log( `TestimonialItem.toggleActive(): Create save request` );
		dto.active = dto.active ? false : true;
		const saveRequest : ctrl.SaveRequest = $.extend( dto, { saveImage:false } );

		common.utils.log( 'TestimonialItem.toggleActive(): Send the save request' );
		const rc2 = await ctrl.save( saveRequest );
		common.html.unblock( self.$blockingDiv );
		if(! rc2.success )
		{
			common.utils.error( 'TestimonialItem.toggleActive(): Error', { rc2 } );
			common.html.showMessage( rc2.errorMessage );
			return;
		}

		common.utils.log( 'TestimonialItem.toggleActive(): Reload page' );
		location.reload();

		common.utils.log( 'TestimonialItem.toggleActive(): END' );
	}

	protected async edit() : Promise<void>
	{
		const self = this;
		common.utils.log( `TestimonialItem.edit(): START: id:'${self.id}'` );

		common.html.block( self.$blockingDiv );
		const rc1 = await ctrl.list({ id:self.id, includeImages:true, includeInactives:true });
		common.html.unblock( self.$blockingDiv );
		if(! rc1 )
		{
			common.utils.error( 'retreive testimonial error', { rc1 } );
			common.html.showMessage( rc1.errorMessage );
			return;	
		}
		if( rc1.testimonials.length != 1 )
		{
			common.html.showMessage( 'Error while retreiving testimonial' );
			return;
		}
		const dto = rc1.testimonials[0];

		formTestimonial.show( dto );

		common.utils.log( `TestimonialItem.edit(): START: id:'${self.id}'` );
	}

	protected async delete_() : Promise<void>
	{
		const self = this;
		common.utils.log( 'TestimonialItem.delete_(): START' );

		common.utils.log( 'TestimonialItem.delete_(): Ask confirmation' );
		const rc1 = await common.html.confirm( message_confirmDelete );
		if(! rc1 )
		{
			common.utils.log( 'TestimonialItem.delete_(): NOT CONFIRMED' );
			return;
		}

		common.utils.log( 'TestimonialItem.delete_(): Launch delete request' );
		common.html.block( self.$blockingDiv );
		const rc2 = await ctrl.delete_( self.id )
		common.html.unblock( self.$blockingDiv );
		if(! rc2.success )
		{
			common.utils.error( 'TestimonialItem.delete_(): Error', { rc2 } );
			common.html.showMessage( rc2.errorMessage );
			return;
		}

		common.utils.log( 'TestimonialItem.delete_(): Reload page' );
		location.reload();

		common.utils.log( 'TestimonialItem.delete_(): END' );
	}
}

class FormTestimonial
{
	private readonly	$container		: JQuery;
	private readonly	$blockingDiv	: JQuery;
	protected readonly	showRequiredText: KnockoutObservable<boolean>;

	private				model			: dtos.Testimonial;

	protected readonly	date			: KnockoutObservable<string>;
	protected readonly	firstLastName	: KnockoutObservable<string>;
	protected readonly	whosWho			: KnockoutObservable<string>;
	protected readonly	text			: KnockoutObservable<string>;
	protected readonly	active			: KnockoutObservable<boolean>;
	protected readonly	imageData		: KnockoutObservable<string>;
	protected readonly	imageSrc		: KnockoutComputed<string>;

	constructor($container:JQuery, $blockingDiv:JQuery, urlImgNotFound:string)
	{
		const self = this;
		this.$container			= $container;
		this.$blockingDiv		= $blockingDiv;
		this.showRequiredText	= ko.observable( false );
		this.model				= null;
		this.date				= ko.observable( '' );
		this.firstLastName		= ko.observable( '' );
		this.whosWho			= ko.observable( '' );
		this.text				= ko.observable( '' );
		this.active				= ko.observable( false );
		this.imageData			= ko.observable( null );
		this.imageSrc			= ko.computed( ()=>
										{
											const data = self.imageData();
											if( data == null )
												return urlImgNotFound;
											return data;
										});

		common.utils.log( 'FormTestimonial(): Bind to the Boostrap\'s close event' );
		self.$container.on( 'hidden.bs.modal', ()=>self.reset() );

		common.utils.log( 'FormTestimonial(): KO bind the form dialog' );
		ko.applyBindings( self, self.$container[0] );
	}

	public show(dto:dtos.Testimonial) : void
	{
		common.utils.log( 'FormTestimonial.show(): START', { dto } );
		const self = this;

		self.reset( dto );
		self.open();

		common.utils.log( 'FormTestimonial.show(): END' );
	}

	public async uploadPicture(files:FileList) : Promise<void>
	{
		common.utils.log( 'FormTestimonial.uploadPicture(): START', {files} );
		const self = this;

		if( files.length == 0 )
			return;
		if( (<any>window).FormData === undefined )
		{
			common.utils.error( "This browser doesn't support HTML5 file uploads!" );
			return;
		}

		common.html.block( self.$blockingDiv );
		const rc = await ctrl.uploadPicture( files[0] );
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'FormTestimonial.uploadPicture(): Error', { rc } );
			common.html.showMessage( /*rc.errorMessage*/message_imageUploadError );
			return;
		}

		self.imageData( rc.imageData );

		common.utils.log( 'FormTestimonial.uploadPicture(): END' );
	}

	private reset(dto?:dtos.Testimonial) : void
	{
		common.utils.log( 'FormTestimonial.reset(): START' );
		const self = this;

		if( dto == null )
		{
			self.model = null;

			self.date( '' );
			self.firstLastName( '' );
			self.whosWho( '' );
			self.text( '' );
			self.active( false );
			self.imageData( null );
		}
		else
		{
			self.model = dto;

			self.date( dto.date );
			self.firstLastName( dto.firstLastName );
			self.whosWho( dto.whosWho );
			self.text( dto.text );
			self.active( dto.active );
			self.imageData( dto.imageData );
		}

		common.utils.log( 'FormTestimonial.reset(): END' );
	}

	private open() : void
	{
		common.utils.log( 'FormTestimonial.open(): START' );
		this.$container.modal( 'show' );
		common.utils.log( 'FormTestimonial.open(): END' );
	}

	protected close() : void
	{
		common.utils.log( 'FormTestimonial.close(): START' );
		this.$container.modal( 'hide' );
		common.utils.log( 'FormTestimonial.close(): END' );
	}

	protected async save() : Promise<void>
	{
		common.utils.log( 'FormTestimonial.save(): START' );
		const self = this;

		if( stringIsNullOrWhitespace( self.firstLastName() )
		 || stringIsNullOrWhitespace( self.whosWho() )
		 || stringIsNullOrWhitespace( self.text() ) )
		{
			self.showRequiredText( true );
			return;
		}

		const fields = {
				date			: self.date(),
				firstLastName	: self.firstLastName(),
				whosWho			: self.whosWho(),
				text			: self.text(),
				active			: self.active(),
				imageData		: self.imageData(),
				saveImage		: true,
			};
		let saveRequest : ctrl.SaveRequest;
		if( self.model == null )
			// add new
			saveRequest = $.extend( { date:'', active:false }, fields );
		else
			// edit existing
			saveRequest = $.extend( self.model, fields );

		common.utils.log( 'FormTestimonial.save(): Send the save request' );
		common.html.block( self.$blockingDiv );
		const rc = await ctrl.save( saveRequest );
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'FormTestimonial.save(): Error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		if( common.pageParameters.isAutenticated )
		{
			common.utils.log( 'FormTestimonial.save(): Refresh page' );  // nb: list may have changed
			location.reload();
		}
		else
		{
			common.utils.log( 'FormTestimonial.save(): Close & show success message' );
			self.close();
			common.html.showMessage( saveSuccessMessage );
		}

		common.utils.log( 'FormTestimonial.save(): END' );
	}
}
