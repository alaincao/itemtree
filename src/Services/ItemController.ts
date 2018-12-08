
import * as common from "../Views/common";
import { Item } from "../DTOs/Item";
import Result from "../Utils/TTTServiceResult";

export async function getUrlCode(originalCode:string) : Promise<string>
{
	if( common.utils.stringIsNullOrWhitespace(originalCode) )
		return null;
	return await common.url.getRequest( common.routes.api.getUrlCode, { originalCode } );
}

export async function details(code:string) : Promise<DetailsResult>
{
	const url = common.routes.api.itemDetails.replace( common.routes.itemCodeParameter, code );
	const rc = await common.url.postRequest<DetailsResult>( url, {} );
	rc.item = rc.result;
	delete rc.result;
	return rc;
}
export interface DetailsResult extends Result
{
	item : Item;
}

export async function save(p:SaveRequest) : Promise<SaveResult>
{
	const rc = await common.url.postRequest<SaveResult>( common.routes.api.itemSave, p );
	rc.newCode = rc.result;
	delete rc.result;
	return rc;
}
export interface SaveRequest
{
	originalCode?	: string;
	item			: Item;
}
export interface SaveResult extends Result
{
	newCode : string;
}

export async function delete_(code:string) : Promise<SaveResult>
{
	const url = common.routes.api.itemDelete.replace( common.routes.itemCodeParameter, code );
	const rc = await common.url.postRequest<SaveResult>( url, {} );
	return rc;
}
