
import * as common from "../Views/common";
import { Result } from "../Utils/TTTServiceResult";
import { Language } from "../Views/common";

export async function get(p:{ itemCode:string, language?:Language }) : Promise<string>
{
	if( p.language == null )
		p.language = common.pageParameters.currentLanguage;
	const rc = <Result>await common.url.postRequestForm( common.routes.api.dynamicPage.get, p );
	return rc.result;
}

export async function update(p:{ itemCode:string, language?:Language, text:string, allowAdd?:boolean }) : Promise<string>
{
	if( p.language == null )
		p.language = common.pageParameters.currentLanguage;
	const rc = <Result>await common.url.postRequestForm( common.routes.api.dynamicPage.update, p );
	return rc.result;
}
