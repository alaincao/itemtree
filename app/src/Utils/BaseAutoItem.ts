
const fieldTagAttribute = 'ttt-name';

export type DictObj	= {[key:string]:any};
export type DictKO	= {[key:string]:KnockoutObservable<any>};

export abstract class BaseAutoItem
{
	private readonly fieldNames : string[];

	constructor($container:JQuery, src:DictObj, fieldNames?:string[])
	{
		this.fieldNames = (fieldNames != null) ? fieldNames : [];
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

		// For each tagged elements we find in '$container'
		$container.find( `[${fieldTagAttribute}]` ).each( (i,e)=>
			{
				// Extract the field name from that attribute
				const $e = $(e);
				const fieldName = $e.attr( fieldTagAttribute );

				if( self.fieldNames.indexOf(fieldName) >= 0 )
					// That field is already already listed
					{/*NOOP*/}
				else
					// Add that field to the fields list
					self.fieldNames.push( fieldName );
			} );

		// For each listed fields ...
		self.fieldNames.forEach( (fieldName)=>
			{
				// ... create a KO observable on 'this' and set it's value using 'src' ...
				dict[ fieldName ] = ko.observable( src[fieldName] );
			} );
	}
}
