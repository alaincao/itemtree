
export interface Routes
{
	languageParameter	: string,
	itemCodeParameter	: string,
	itemIDParameter		: string,

	api	: {
			login			: string,
			logout			: string,
			changePassword	: string,
			redirections	: string,
			tree : {
					sanitizePath: string,
					sanitizeName: string,
					operations	: string,
					file		: string,
					image		: string,
				},
		},
}
