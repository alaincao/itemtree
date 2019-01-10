
export interface Routes
{
	languageParameter	: string,
	itemCodeParameter	: string,

	itemTTT : {
			list	: string,
			edit	: string,
		},
	api	: {
			getUrlCode		: string,
			login			: string,
			logout			: string,
			changePassword	: string,
			autoComplete	: string,
			translations : {
					list	: string,
					save	: string,
				},
			items : {
					list			: string,
					details			: string,
					save			: string,
					delete			: string,
					detailsPictures	: string,
					pictures : {
							delete	: string,
							reorder	: string,
							setMain	: string,
							upload	: string,
						},
				},
			blog : {
					list			: string,
					pictureUpload	: string,
				},
		},
}
