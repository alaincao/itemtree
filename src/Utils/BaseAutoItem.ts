
const fieldTagAttribute = 'ttt-name';

export type DictObj	= {[key:string]:any};
export type DictKO	= {[key:string]:KnockoutObservable<any>};

export abstract class BaseAutoItem
{
	private readonly fieldNames : string[];

	constructor($container:JQuery, src:DictObj)
	{
		this.fieldNames = [];
		this.knockoutify( $container, src );
	}

	public toDictObj(dict?:DictObj) : DictObj
	{
		const self = this;
		const dictKO : DictKO = <any>self;
		if( dict == null )
			dict = {};

		$.each( self.fieldNames, (i,fieldName)=>
			{
				const observable = dictKO[ fieldName ];
				dict[ fieldName ] = observable();
			} );

		return dict;
	}

	public fromDictObj(dict:DictObj) : void
	{
		const self = this;
		const dictKO : DictKO = <any>self;
		$.each( self.fieldNames, (i,fieldName)=>
			{
				const observable = dictKO[ fieldName ];
				observable( dict[fieldName] );
			} );
	}

	private knockoutify($container:JQuery, src:DictObj) : void
	{
		const self = this;
		const dict : DictKO = <any>this;

		// For each tagged elements we find in '$container' ...
		$container.find( `[${fieldTagAttribute}]` ).each( (i,e)=>
			{
				const $e = $(e);
				const fieldName = $e.attr( fieldTagAttribute );

				// ... create a KO observable on 'this' and set it's value using 'src' ...
				dict[ fieldName ] = ko.observable( src[fieldName] );

				// ... and register this field to 'fieldNames'
				self.fieldNames.push( fieldName );
			} );
	}
}
