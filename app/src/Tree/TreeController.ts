
import * as common from "../Views/common"
import * as helper from "./TreeHelper";
import Result from "../Utils/TTTServiceResult";

export const pathSeparator = '/';

export interface MetaData
{
	type?	: helper.Type;

	[key:string]:any;
}

//////////

export function getPathSegments(path:string) : string[]
{
	if( path == null )
		path = '';
	if( path.indexOf(pathSeparator) == 0 )
		// Remove first '/'
		path = path.substring( 1 );
	const segments = path.split( pathSeparator );
	if( (segments.length == 1) && (segments[0] == '') )
		// Root path
		return [];
	return segments;
}

//////////

export function sanitizeName(name:string) : Promise<string>
{
	return common.url.getRequest( common.pageParameters.routes.api.tree.sanitizeName, {name} )
}

export function sanitizePath(path:string) : Promise<string>
{
	return common.url.getRequest( common.pageParameters.routes.api.tree.sanitizePath, {path} )
}

//////////

export async function fileSave(path:string, file:File, contentType?:string) : Promise<FileSaveResult>
{
	const url = common.routes.api.tree.file;
	const formData = new FormData();
	formData.append( 'path', path );
	formData.append( 'file', file );
	if( contentType )
		formData.append( 'contentType', contentType );
	const response = await common.url.postRequestFormData<FileSaveResult>( url, formData );
	if( response.success )
	{
		response.name			= response.result.name;
		response.dir			= response.result.dir;
		response.path			= response.result.path;
		response.fileName		= response.result.fileName;
		response.contentType	= response.result.contentType;
		delete response.result;
	}
	return response;
}
export interface FileSaveResult extends Result
{
	name		: string;
	dir			: string;
	path		: string;
	fileName	: string;
	contentType	: string;
}

//////////

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

export async function getChildNodes(path:string) : Promise<string[]>
{
	const rv = await operations([{ getChildNodes:{path} }]);
	if(! rv.success )
	{
		common.utils.error( 'GetChildNodes operation failed', { rv } );
		throw `GetChildNodes operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].childNodes;
}

//////////

export async function getNodeMetaData(path:string) : Promise<{[key:string]:any}>
{
	const rv = await operations([{ getNodeMetaData:{path} }]);
	if(! rv.success )
	{
		common.utils.error( 'GetNodeMetaData operation failed', { rv } );
		throw `GetNodeMetaData operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].metaData;
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

export async function setNodeMetaData(path:string, metaData:MetaData, expectedType?:string) : Promise<string>
{
	const rv = await operations([{ setNodeMetaData:{path,metaData,expectedType} }]);
	if(! rv.success )
	{
		common.utils.error( 'SetNodeMetaData operation failed', { rv } );
		throw `SetNodeMetaData operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].path;
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

export async function getOrCreateNode(path:string, expectedType?:string, data?:any) : Promise<string>
{
	const rv = await operations([{ getOrCreateNode:{path,expectedType,data} }]);
	if(! rv.success )
	{
		common.utils.error( 'GetOrCreateNode operation failed', { rv } );
		throw `GetOrCreateNode operation at '${path}' failed: ${rv.errorMessage}`;
	}
	return rv.responses[0].path;
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

export async function restoreTree(path:string, filePath:string, overwrite?:boolean) : Promise<string>
{
	const rv = await operations([{ restoreTree:{path,filePath,overwrite} }]);
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
		getChildNodes?		: GetChildNodes;
		getNodeMetaData?	: GetNodeMetaData;
		getNodeData?		: GetNodeData;
		setNodeMetaData?	: SetNodeMetaData;
		setNodeData?		: SetNodeData;
		getOrCreateNode?	: GetOrCreateNode;
		delTree?			: DelTree;
		restoreTree?		: RestoreTree;
	}
	export interface Response
	{
		path			: string;
		childNodes?		: string[];
		metaData?		: {[key:string]:any};
		data?			: string;
		affectedRows?	: number;
	}

	export interface GetChildNodes
	{
		path			: string;
	}
	export interface GetNodeMetaData
	{
		path			: string;
	}
	export interface GetNodeData
	{
		path			: string;
	}
	export interface SetNodeMetaData
	{
		path			: string;
		expectedType?	: string;
		metaData		: MetaData;
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
		overwrite?		: boolean;
	}
}
