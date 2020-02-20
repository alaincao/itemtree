
import { Language } from "./Language";
import { Routes } from "./Routes";

export interface PageParameters
{
	pageTitle		: string,
	isAutenticated	: boolean,
	currentLanguage	: Language,
	languageUrls	: {[key:string]:string},
	routes			: Routes,
	hasErrors		: boolean,
	isDebug			: boolean,
	logs			: string[],
}
