
import * as common from "../Views/common";
import * as dto from "../DTOs/Testimonial";
import Result from "../Utils/TTTServiceResult";

export async function list(p:ListRequest) : Promise<ListResponse>
{
	const rc = <ListResponse>await common.url.postRequestForm( common.routes.api.testimonial.list, p );
	rc.testimonials = rc.result;
	delete rc.result;
	return rc;
}
export interface ListRequest
{
	includeImages?		: boolean;
	includeInactives?	: boolean;
	id?					: number;
}
export interface ListResponse extends Result
{
	testimonials : dto.Testimonial[];
}

export async function save(post:SaveRequest) : Promise<Result>
{
	const rc = await common.url.postRequestJSON<Result>( common.routes.api.testimonial.save, post );
	return rc;
}
export interface SaveRequest extends dto.Testimonial
{
	saveImage	: boolean;
}

export async function delete_(id:number) : Promise<Result>
{
	const rc = await common.url.postRequestForm<Result>( common.routes.api.testimonial.delete, { id } );
	return rc;
}

export async function uploadPicture(file:File) : Promise<UploadPictureResult>
{
	const url = common.routes.api.testimonial.pictureUpload;
	const formData = new FormData();
	formData.append( 'file', file );

	const response = await common.url.postRequestFormData<UploadPictureResult>( url, formData );
	response.imageData = response.result;
	delete response.result;

	return response;
}
export interface UploadPictureResult extends Result
{
	imageData : string;
}
