
import * as common from "../Views/common"
import Result from "../Utils/TTTServiceResult";

export async function imageSave(path:string, file:File) : Promise<ImageSaveResult>
{
	const url = common.routes.api.tree.image;
	const formData = new FormData();
	formData.append( 'path', path );
	formData.append( 'file', file );
	const response = await common.url.postRequestFormData<ImageSaveResult>( url, formData );
	if( response.success )
	{
		response.name	= response.result.name;
		response.dir	= response.result.dir;
		response.path	= response.result.path;
		delete response.result;
	}
	return response;
}
export interface ImageSaveResult extends Result
{
	name	: string;
	dir		: string;
	path	: string;
}

//////////

export async function operations(ops:operations.Operation[]) : Promise<OperationsResult>
{
	const response = await common.url.postRequestJSON<OperationsResult>( common.routes.api.tree.operations, ops );
	if( response.success )
	{
		response.responses = response.result;
		delete response.result;
	}
	return response;
}
export interface OperationsResult extends Result
{
	responses	: operations.Response[];
}
export namespace operations
{
	export interface Operation
	{
		getOrCreateNode? : GetOrCreateNode;
	}
	export interface Response
	{
		path : string;
	}

	export interface GetOrCreateNode
	{
		path			: string;
		expectedType?	: string;
		data?			: any;
	}
}
