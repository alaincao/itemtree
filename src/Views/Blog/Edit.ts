
import * as common from "../common";
import * as ctrl from "../../Services/BlogController";

export async function init(p:{	$picUploadControl	: JQuery,
							}) : Promise<void>
{
	common.utils.log( 'edit.init() START', { p } );

	common.utils.log( 'edit.init(): Init picture upload' );
	p.$picUploadControl.on( 'change', ()=>
		{
			const files = (<HTMLInputElement>p.$picUploadControl[0]).files;
			uploadPicture( files );
		} );

	common.utils.log( 'edit.init() END' );
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

	 $('#foobar').html( rc.imageTagContent );
 
	common.utils.log( 'edit.uploadPicture(): END' );
}
