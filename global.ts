
import * as common		from "./src/Views/common";
import * as itemlist	from "./src/Views/ItemTTT/List";
import * as itemedit	from "./src/Views/ItemTTT/Edit";

///////

// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt =
{
	common: common,
	itemttt: {
		list	: itemlist,
		edit	: itemedit,
	},
}
declare global
{
	interface Window { ttt: typeof ttt; }
}
window.ttt = ttt;
