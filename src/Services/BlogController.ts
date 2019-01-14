
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";
import * as dto from "../DTOs/BlogPost";

export async function list(p:ListRequest) : Promise<ListResponse>
{
	const rc = <ListResponse>await common.url.postRequestForm( common.routes.api.blog.list, p );
	rc.posts = rc.result;
	delete rc.result;
	return rc;
}
export interface ListRequest
{
	includeImages?		: boolean;
	includeInactives?	: boolean;
	id?					: number;
	skipToID?			: number;
	skip?				: number;
	take?				: number;
}
export interface ListResponse extends Result
{
	posts : dto.BlogPost[];
}

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

export async function save(post:dto.BlogPost) : Promise<SaveResponse>
{
	const rc = await common.url.postRequestJSON<SaveResponse>( common.routes.api.blog.save, post );
	rc.id = rc.result;
	delete rc.result;
	return rc;
}
export interface SaveResponse extends Result
{
	id : number;
}

export async function delete_(id:number) : Promise<Result>
{
	const rc = await common.url.postRequestForm<Result>( common.routes.api.blog.delete, { id } );
	return rc;
}
