
import * as common		from "./src/Views/common";
import * as treeCtrl	from "./src/Tree/TreeController";
import * as treeTags	from "./src/Tree/TagHelpers";
import * as treeHelper	from "./src/Tree/TreeHelper";
import * as adminhome	from "./src/Views/Admin/Home";
import * as adminRedir	from "./src/Views/Admin/Redirections";

///////

// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt =
{
	common: common,
	admin: {
		home			: adminhome,
		redirections	: adminRedir,
	},
	tree: {  // nb: Only for use from the console (i.e. not used directly by pages' HTML)
		controller	: treeCtrl,
		tags		: treeTags,
		helper		: treeHelper,
	},
}
declare global
{
	interface Window { ttt: typeof ttt; }
}
window.ttt = ttt;
