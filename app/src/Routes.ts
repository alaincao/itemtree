
export interface Routes
{
	languageParameter	: string,
	itemCodeParameter	: string,
	itemIDParameter		: string,

	itemTTT : {
			list	: string,
			edit	: string,
		},
	blog : {
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
					save			: string,
					delete			: string,
					pictureUpload	: string,
				},
			testimonial : {
					list			: string,
					save			: string,
					delete			: string,
					pictureUpload	: string,
				},
			dynamicPage : {
					get		: string,
					update	: string,
				},
			tree : {
					operations	: string,
					image		: string,
				},
		},
}
