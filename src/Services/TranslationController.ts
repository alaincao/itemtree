
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";

export const { e:TranslationTypes, a:allTranslationTypes } = common.utils.strEnum([ 'option', 'feature' ]);
export type TranslationType = keyof typeof TranslationTypes;

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
