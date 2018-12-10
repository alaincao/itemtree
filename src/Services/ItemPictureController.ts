
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";

export function upload(itemCode:string, file:File) : Promise<UploadResult>
{
	const url = common.routes.api.itemPictureUpload.replace( common.routes.itemCodeParameter, itemCode );
	const formData = new FormData();
	formData.append( 'file', file );
	return new Promise<UploadResult>( (resolve,reject)=>
		{
			$.ajax( {	type		: "POST",
						url			: url,
						contentType	: false,
						processData	: false,
						data		: formData,
						dataType	: 'json',
						success		: (data:UploadResult,textStatus,jqXHR)=>
										{
											data.imageNumber = data.result;
											delete data.result;
											resolve( data );
										},
						error		: (jqXHR,textStatus,errorThrown)=>
										{
											reject( textStatus );
										},
					} );
		} );
}
export interface UploadResult extends Result
{
	imageNumber : number;
}
