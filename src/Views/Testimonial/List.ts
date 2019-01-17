
import * as common from "../common";
import * as ctrl from "../../Services/TestimonialController";

export var formAddNew : FormAddNew;

export function init(p:{	urlImgNotFound		: string,
							$picUploadControl	: JQuery }) : void
{
	common.utils.log( 'list.init(): START' );

	common.utils.log( 'list.init(): Create KO for new testimonial form' );
	formAddNew = new FormAddNew( p.urlImgNotFound );

	common.utils.log( 'list.init(): Init picture upload' );
	p.$picUploadControl.on( 'change', ()=>
		{
			const files = (<HTMLInputElement>p.$picUploadControl[0]).files;
			/*await*/ formAddNew.uploadPicture( files );
		} );

	common.utils.log( 'list.init(): END' );
}

class FormAddNew
{
	private readonly	imageData	: KnockoutObservable<string>;
	protected readonly	imageSrc	: KnockoutComputed<string>;

	constructor(urlImgNotFound:string)
	{
		const self = this;
		this.imageData	= ko.observable( null );
		this.imageSrc	= ko.computed( ()=>
								{
									const data = self.imageData();
									if( data == null )
										return urlImgNotFound;
									return data;
								});
	}

	public async uploadPicture(files:FileList) : Promise<void>
	{
		common.utils.log( 'AddNewForm.uploadPicture(): START', {files} );
		const self = this;

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
			common.utils.error( 'AddNewForm.uploadPicture(): Error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		self.imageData( rc.imageData );

		common.utils.log( 'AddNewForm.uploadPicture(): END' );
	}

	protected save() : void
	{
		common.utils.error( 'TODO: FormAddNew.save()' );
	}
}
