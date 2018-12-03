
import * as common		from "./src/Views/common";
import * as itemlist	from "./src/Views/ItemTTT/List";

///////

// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt =
{
	common: common,
	itemttt: {
		list : itemlist,
	},
}
declare global
{
	interface Window { ttt: typeof ttt; }
}
window.ttt = ttt;
