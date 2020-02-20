
export interface BlogPost
{
	id?			: number;
	date		: string;
	imageHtml	: string;
	titleHtmlEN	: string;
	titleHtmlFR	: string;
	titleHtmlNL	: string;
	textHtmlEN	: string;
	textHtmlFR	: string;
	textHtmlNL	: string;
	active		: boolean;
}

export class BlogPostKO
{
	public readonly	id			: KnockoutObservable<number>;
	public readonly	date		: KnockoutObservable<string>;
	public readonly	imageHtml	: KnockoutObservable<string>;
	public readonly	titleHtmlEN	: KnockoutObservable<string>;
	public readonly	titleHtmlFR	: KnockoutObservable<string>;
	public readonly	titleHtmlNL	: KnockoutObservable<string>;
	public readonly	textHtmlEN	: KnockoutObservable<string>;
	public readonly	textHtmlFR	: KnockoutObservable<string>;
	public readonly	textHtmlNL	: KnockoutObservable<string>;
	public readonly	active		: KnockoutObservable<boolean>;

	constructor(src?:BlogPost)
	{
		const self = this;

		this.id				= ko.observable( null );
		this.date			= ko.observable( '' );
		this.imageHtml		= ko.observable( '' );
		this.titleHtmlEN	= ko.observable( '' );
		this.titleHtmlFR	= ko.observable( '' );
		this.titleHtmlNL	= ko.observable( '' );
		this.textHtmlEN		= ko.observable( '' );
		this.textHtmlFR		= ko.observable( '' );
		this.textHtmlNL		= ko.observable( '' );
		this.active			= ko.observable( false );

		if( src != null )
			self.fromDTO( src );
	}

	public fromDTO(src:BlogPost) : void
	{
		const self = this;

		self.id				( src.id );
		self.date			( src.date );
		self.imageHtml		( src.imageHtml );
		self.titleHtmlEN	( src.titleHtmlEN );
		self.titleHtmlFR	( src.titleHtmlFR );
		self.titleHtmlNL	( src.titleHtmlNL );
		self.textHtmlEN		( src.textHtmlEN );
		self.textHtmlFR		( src.textHtmlFR );
		self.textHtmlNL		( src.textHtmlNL );
		self.active			( src.active );
	}

	public toDTO() : BlogPost
	{
		const self = this;
		const dst : BlogPost = <any>{};

		dst.id			= self.id();
		dst.date		= self.date();
		dst.imageHtml	= self.imageHtml();
		dst.titleHtmlEN	= self.titleHtmlEN();
		dst.titleHtmlFR	= self.titleHtmlFR();
		dst.titleHtmlNL	= self.titleHtmlNL();
		dst.textHtmlEN	= self.textHtmlEN();
		dst.textHtmlFR	= self.textHtmlFR();
		dst.textHtmlNL	= self.textHtmlNL();
		dst.active		= self.active();

		return dst;
	}
}
