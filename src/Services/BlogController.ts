
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";

export async function uploadPicture(file:File) : Promise<UploadPictureResult>
{
	const url = common.routes.api.blog.pictureUpload;
	const formData = new FormData();
	formData.append( 'file', file );

	const response = await common.url.postRequestFormData<UploadPictureResult>( url, formData );
	response.imageTagContent = response.result;
	delete response.result;

	return response;
}
export interface UploadPictureResult extends Result
{
	imageTagContent : string;
}
