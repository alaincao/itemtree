
import { strEnum } from "../utils";
import * as common from "../common";

export namespace list
{
	export const { e:ViewModes, a:allViewModes } = strEnum([ 'grid', 'list' ]);
	export type ViewMode = keyof typeof ViewModes;

	export const { e:SortingFields, a:allSortingFields } = strEnum([ 'price', 'price_desc', 'name', 'name_desc' ]);
	export type SortingField = keyof typeof SortingFields;

	export async function getListContent(p:GetListContentRequest) : Promise<string>
	{
		var url = common.routes.itemTTT.list.replace( common.routes.languageParameter, common.pageParameters.currentLanguage );
		return await common.url.getRequest( url, p );
	}
	export interface GetListContentRequest
		{
			viewMode?		: ViewMode,
			sortingField?	: SortingField,
			noLayout?		: boolean,
		};
}
