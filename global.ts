
import * as common from './Views/common';

///////

// Global assignment of window.itemttt (will be available in each pages e.g. from the console):
var itemttt =
{
	common: common,
	api: {
	},
}
declare global
{
	interface Window { itemttt: typeof itemttt; }
}
window.itemttt = itemttt;
