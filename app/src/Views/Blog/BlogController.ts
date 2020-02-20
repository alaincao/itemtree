
import * as common from "../common";

export namespace list
{
	export async function list(p:{ noLayout?:boolean, includeImages?:boolean, includeInactives?:boolean, skipToID?:number, take?:number }) : Promise<string>
	{
		var url = common.routes.blog.list.replace( common.routes.languageParameter, common.pageParameters.currentLanguage );
		return await common.url.getRequest( url, p );
	}
}
