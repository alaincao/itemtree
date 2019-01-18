
import * as common from "../Views/common";
import * as dto from "../DTOs/Testimonial";
import Result from "../Utils/TTTServiceResult";

export async function save(post:dto.Testimonial) : Promise<Result>
{
	const rc = await common.url.postRequestJSON<Result>( common.routes.api.testimonial.save, post );
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
