
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
			autoComplete		: string,
			itemsListing		: string,
			itemDetails			: string,
			itemSave			: string,
			itemDelete			: string,
			itemDetailsPictures	: string,
			itemPictureDelete	: string,
			itemPictureReorder	: string,
			itemPictureSetMain	: string,
			itemPictureUpload	: string,
		},
}
