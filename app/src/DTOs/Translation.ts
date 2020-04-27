
import { stringIsNullOrWhitespace } from "../Views/utils";

export interface Translation
{
	en : string;
	fr : string;
	nl : string;
}
export interface TranslationItem extends Translation
{
	inOriginal		: boolean;
	inTranslation	: boolean;
}

export class TranslationKO
{
	public readonly en	: KnockoutObservable<string>;
	public readonly fr	: KnockoutObservable<string>;
	public readonly nl	: KnockoutObservable<string>;

	constructor(src?:Translation)
	{
		if( src == null )
			src = { en:'', fr:'', nl:'' };
		this.en = ko.observable( src.en );
		this.fr = ko.observable( src.fr );
		this.nl = ko.observable( src.nl );
	}
}
export class TranslationItemKO extends TranslationKO
{
	public readonly inOriginal		: KnockoutObservable<boolean>;
	public readonly inTranslation	: KnockoutObservable<boolean>;

	public readonly enOriginal	: KnockoutObservable<string>;
	public readonly frOriginal	: KnockoutObservable<string>;
	public readonly nlOriginal	: KnockoutObservable<string>;
	public readonly modified	: KnockoutComputed<boolean>;

	constructor(src?:TranslationItem)
	{
		if( src == null )
			src = { en:'', fr:'', nl:'', inOriginal:false, inTranslation:false };
		super( src );
		const self = this;

		this.inOriginal		= ko.observable( src.inOriginal	 );
		this.inTranslation	= ko.observable( src.inTranslation );

		this.enOriginal = ko.observable( self.en() );
		this.frOriginal = ko.observable( self.fr() );
		this.nlOriginal = ko.observable( self.nl() );
		this.modified = ko.computed( ()=>
							{
								if( self.en() != self.enOriginal() )	return true;
								if( self.fr() != self.frOriginal() )	return true;
								if( self.nl() != self.nlOriginal() )	return true;
								return false;
							} );
	}

	public toDTO() : Translation
	{
		return {
				en : this.en(),
				fr : this.fr(),
				nl : this.nl(),
			};
	}

	public setAllLanguagesTo(str:string) : void
	{
		this.en( str );
		this.fr( str );
		this.nl( str );
	}

	public copyEnToBlanks() : void
	{
		const self = this;
		const en = self.en();
		if( stringIsNullOrWhitespace(self.fr()) )
			self.fr( en );
		if( stringIsNullOrWhitespace(self.nl()) )
			self.nl( en );
	}
}
