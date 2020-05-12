
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

export async function getNodeData(path:string) : Promise<string>
{
	const rv = await operations([{ getNodeData:{path} }]);
	if(! rv.success )
	{
		common.utils.error( 'GetNodeData operation failed', { rv } );
		throw `GetNodeData operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].data;
}

//////////

export async function setNodeData(path:string, data:any) : Promise<void>
{
	const rv = await operations([{ setNodeData:{path,data} }]);
	if(! rv.success )
	{
		common.utils.error( 'SetNodeData operation failed', { rv } );
		throw `SetNodeData operation at '${path}' failed: ${rv.errorMessage}`;
	}
}

//////////

export async function delTree(path:string, included?:boolean) : Promise<number>
{
	const rv = await operations([{ delTree:{path,included} }]);
	if(! rv.success )
	{
		common.utils.error( 'DelTree operation failed', { rv } );
		throw `DelTree operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].affectedRows;
}

//////////

export async function restoreTree(path:string, filePath:string) : Promise<string>
{
	const rv = await operations([{ restoreTree:{path,filePath} }]);
	if(! rv.success )
	{
		common.utils.error( 'RestoreTree operation failed', { rv } );
		throw `RestoreTree operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].path;
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
		getNodeData?		: GetNodeData;
		setNodeData?		: SetNodeData;
		getOrCreateNode?	: GetOrCreateNode;
		delTree?			: DelTree;
		restoreTree?		: RestoreTree;
	}
	export interface Response
	{
		path			: string;
		data?			: string;
		affectedRows?	: number;
	}

	export interface GetNodeData
	{
		path			: string;
	}
	export interface SetNodeData
	{
		path			: string;
		expectedType?	: string;
		data			: any;
	}
	export interface GetOrCreateNode
	{
		path			: string;
		expectedType?	: string;
		data?			: any;
	}
	export interface DelTree
	{
		path			: string;
		included?		: boolean;
	}
	export interface RestoreTree
	{
		path			: string;
		filePath		: string;
	}
}