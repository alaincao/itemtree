
export interface Translation
{
	en : string;
	fr : string;
	nl : string;
}

export class TranslationKO
{
	public readonly en	: KnockoutObservable<string>;

	constructor(src?:Translation)
	{
		if( src == null )
			src = <Translation>{ en:'' };
		this.en = ko.observable( src.en );
	}
}
