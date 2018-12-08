
export interface Routes
{
	languageParameter	: string,
	itemCodeParameter	: string,

	itemTTT : {
			itemsList	: string,
			itemEdit	: string,
		},
	api	: {
			getUrlCode			: string,
			login				: string,
			logout				: string,
			changePassword		: string,
			itemsListing		: string,
			itemDetails			: string,
			itemSave			: string,
			itemDelete			: string,
			itemDetailsPictures	: string,
		},
}
