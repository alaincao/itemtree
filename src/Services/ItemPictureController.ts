
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";

export function upload(itemCode:string, file:File) : Promise<UploadResult>
{
	const url = common.routes.api.items.pictures.upload.replace( common.routes.itemCodeParameter, itemCode );
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

export async function delete_(p:{ itemCode:string, number:number }) : Promise<Result>
{
	const url = common.routes.api.items.pictures.delete.replace( common.routes.itemCodeParameter, p.itemCode );
	const rc = await common.url.postRequestForm<Result>( url, { number:p.number } );
	return rc;
}

export async function reorder(p:{ itemCode:string, number:number, newNumber:number }) : Promise<Result>
{
	const url = common.routes.api.items.pictures.reorder.replace( common.routes.itemCodeParameter, p.itemCode );
	const rc = await common.url.postRequestForm<Result>( url, { number:p.number, newNumber:p.newNumber } );
	return rc;
}

export async function setMain(p:{ itemCode:string, number:number, isMain:boolean }) : Promise<Result>
{
	const url = common.routes.api.items.pictures.setMain.replace( common.routes.itemCodeParameter, p.itemCode );
	const rc = await common.url.postRequestForm<Result>( url, { number:p.number, isMain:p.isMain } );
	return rc;
}
