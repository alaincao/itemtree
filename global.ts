
import * as common		from "./src/Views/common";
import * as adminhome	from "./src/Views/Admin/Home";
import * as itemlist	from "./src/Views/ItemTTT/List";
import * as itemadd		from "./src/Views/ItemTTT/Add";
import * as itemedit	from "./src/Views/ItemTTT/Edit";
import * as bloglist	from "./src/Views/Blog/List";
import * as blogedit	from "./src/Views/Blog/Edit";
import * as tstmlist	from "./src/Views/Testimonial/List";

///////

// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var ttt =
{
	common: common,
	admin: {
		home : adminhome,
	},
	itemttt: {
		list	: itemlist,
		add		: itemadd,
		edit	: itemedit,
	},
	blog: {
		list	: bloglist,
		edit	: blogedit,
	},
	testimonial: {
		list	: tstmlist,
	},
}
declare global
{
	interface Window { ttt: typeof ttt; }
}
window.ttt = ttt;
