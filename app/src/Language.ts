
import { strEnum, stringIsNullOrWhitespace } from "./Views/utils";
import * as common from "./Views/common";

export const { e:Languages, a:allLanguages } = strEnum([ 'en', 'fr', 'nl' ]);
export type Language = keyof typeof Languages;

export class Translation
{
	public readonly items		: {[key in Language]:common.utils.TrackedObservable<string>};
	public readonly trackable	: common.utils.TrackedObservable<{[key in Language]:string}>;  // <== READ ONLY !
	public readonly current		: KnockoutObservable<string>;

	constructor(currentLanguage?:KnockoutObservable<Language>)
	{
		const self = this;

		this.items = <any>{};
		for( const l of allLanguages )
			self.items[ l ] = common.utils.observable( ko.observable('') );

		const trackable = ko.pureComputed( ()=>
			{
				const rv : {[key in Language]:string} = <any>{};
				for( const l of allLanguages )
					rv[ l ] = self.items[ l ]();
				return rv;
			} );
		this.trackable = $.extend( trackable, {
				hasChanges : ko.pureComputed( ()=>
					{
						for( const l of allLanguages )
							if( self.items[ l ].hasChanges() )
								return true;
						return false;
					} ),
				setInitial : ()=>
					{
						for( const l of allLanguages )
							self.items[ l ].setInitial();
					},
				reset : ()=>self.resetValues()
			} );

		if( currentLanguage == null )
			currentLanguage = ko.observable( common.pageParameters.currentLanguage );
		this.current = ko.pureComputed({
				read	: ()=>self.items[currentLanguage()](),
				write	: (value)=>self.items[currentLanguage()]( value ),
			});
	}

	public resetValues(values?:{[key in Language]:string}) : void
	{
		const self = this;
		values = values ?? <any>{};

		for( const l of allLanguages )
		{
			const value = values[ l ];
			if( stringIsNullOrWhitespace(value) )
				self.items[ l ]( '' );
			else
				self.items[ l ]( value );
			self.items[ l ].setInitial();
		}
	}

	public getValues() : {[key in Language]:string}
	{
		const self = this;

		const values : {[key in Language]:string} = <any>{};
		for( const l of allLanguages )
			values[ l ] = self.items[ l ]();
		return values;
	}
}
