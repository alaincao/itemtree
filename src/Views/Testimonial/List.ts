
import * as common from "../common";
import * as dtos from "../../DTOs/Testimonial";
import * as ctrl from "../../Services/TestimonialController";

export var formTestimonial : FormTestimonial;
var saveSuccessMessage : string;

export function init(p:{	urlImgNotFound		: string,
							saveSuccessMessage	: string,
							$formDialog			: JQuery,
							$picUploadControl	: JQuery }) : void
{
	common.utils.log( 'list.init(): START' );

	saveSuccessMessage = p.saveSuccessMessage;

	common.utils.log( 'list.init(): Create KO for new testimonial form' );
	formTestimonial = new FormTestimonial( p.$formDialog, p.urlImgNotFound );

	common.utils.log( 'list.init(): Init picture upload' );
	p.$picUploadControl.on( 'change', ()=>
		{
			const files = (<HTMLInputElement>p.$picUploadControl[0]).files;
			/*await*/ formTestimonial.uploadPicture( files );
		} );

	common.utils.log( 'list.init(): END' );
}

class FormTestimonial
{
	private readonly	$container		: JQuery;
	private readonly	$blockingDiv	: JQuery;
	protected readonly	showRequiredText: KnockoutObservable<boolean>;

	private				model			: dtos.Testimonial;

	protected readonly	firstLastName	: KnockoutObservable<string>;
	protected readonly	whosWho			: KnockoutObservable<string>;
	protected readonly	text			: KnockoutObservable<string>;
	private readonly	imageData		: KnockoutObservable<string>;
	protected readonly	imageSrc		: KnockoutComputed<string>;

	constructor($container:JQuery, urlImgNotFound:string)
	{
		const self = this;
		this.$container			= $container;
		this.$blockingDiv		= $container;
		this.showRequiredText	= ko.observable( false );
		this.model				= null;
		this.firstLastName		= ko.observable( '' );
		this.whosWho			= ko.observable( '' );
		this.text				= ko.observable( '' );
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
			common.html.showMessage( rc.errorMessage );
			return;
		}

		self.imageData( rc.imageData );

		common.utils.log( 'FormTestimonial.uploadPicture(): END' );
	}

	private reset() : void
	{
		common.utils.log( 'FormTestimonial.reset(): START' );
		const self = this;

		self.showRequiredText( false );
		self.model = null;
		self.firstLastName( '' );
		self.whosWho( '' );
		self.text( '' );
		self.imageData( null );

		common.utils.log( 'FormTestimonial.reset(): END' );
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

		if( common.utils.stringIsNullOrWhitespace( self.firstLastName() )
		 || common.utils.stringIsNullOrWhitespace( self.whosWho() )
		 || common.utils.stringIsNullOrWhitespace( self.text() ) )
		{
			self.showRequiredText( true );
			return;
		}

		const fields = {
				firstLastName	: self.firstLastName(),
				whosWho			: self.whosWho(),
				text			: self.text(),
				imageData		: self.imageData(),
			};
		if( self.model == null )
			// Add new
			self.model = $.extend( { date:'', active:false }, fields );
		else
			// Edit openned one
			$.extend( self.model, fields );

		common.utils.log( 'FormTestimonial.save(): Send the save request' );
		common.html.block( self.$blockingDiv );
		const rc = await ctrl.save( self.model );
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'FormTestimonial.save(): Error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		self.close();

		common.html.showMessage( saveSuccessMessage );

		common.utils.log( 'FormTestimonial.save(): END' );
	}
}
