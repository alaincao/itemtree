
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";
import * as dto from "../DTOs/Translation";

export const { e:TranslationTypes, a:allTranslationTypes } = common.utils.strEnum([
																	'feature',
																]);
export type TranslationType = keyof typeof TranslationTypes;

export async function list(p:ListRequest) : Promise<ListResponse>
{
	const rc = <ListResponse>await common.url.postRequestForm( common.routes.api.translations.list, p );
	rc.translations = rc.result;
	delete rc.result;
	return rc;
}
export interface ListRequest
{
	type					: TranslationType;
	includeOriginals?		: boolean;
	includeTranslations?	: boolean;
}
export interface ListResponse extends Result
{
	translations : dto.TranslationItem[];
}

export async function save(p:SaveRequest) : Promise<Result>
{
	return await common.url.postRequestJSON<Result>( common.routes.api.translations.save, p );
}
export interface SaveRequest
{
	type			: TranslationType;
	translations	: SaveRequestItem[];
}
export interface SaveRequestItem extends dto.Translation
{
	enOriginal?	: string;
}

export async function autoCompleteResolve(p:AutoCompleteResolveRequest) : Promise<AutoCompleteResolveResponse>
{
	const url = common.routes.api.autoComplete;
	const rc = <AutoCompleteResolveResponse>await common.url.postRequestForm( url, p );
	rc.list = rc.result;
	delete rc.result;
	return rc;
}
export interface AutoCompleteResolveRequest
{
	type					: TranslationType;
	searchString?			: string;
	includeTranslations?	: boolean;
}
export interface AutoCompleteResolveResponse extends Result
{
	list : string[];
}
