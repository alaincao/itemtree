
import * as common from "./../Views/common";
import { jsonStringify } from "../Views/utils";
import { Result } from "../Utils/TTTServiceResult";

export interface Entry
{
	source			: string;
	destination		: string;
	useRegex?		: boolean;
}

export class EntryKO
{
	public readonly		source			: KnockoutObservable<string>;
	public readonly		destination		: KnockoutObservable<string>;
	public readonly		useRegex		: KnockoutObservable<boolean>;

	constructor(src:Entry=null)
	{
		const self = this;
		this.source			= ko.observable( '' );
		this.destination	= ko.observable( '' );
		this.useRegex		= ko.observable( false );

		common.utils.ensureString({ observable:self.source, canBeNull:false });
		common.utils.ensureString({ observable:self.destination, canBeNull:false });
		common.utils.ensureBoolean({ observable:self.useRegex });

		if( src != null )
		{
			self.source( src.source );
			self.destination( src.destination );
			self.useRegex( src.useRegex );
		}
	}

	public toModel() : Entry
	{
		const self = this;

		const dst = <Entry>{};
		dst.source		= self.source();
		dst.destination	= self.destination();
		if( self.useRegex() )  // nb: do not set if 'false' for brevity
			dst.useRegex	= true;
		return dst;
	}
}

//////////

export async function save(entries:Entry[]) : Promise<Result>
{
	const json = jsonStringify( entries );
	const url = common.routes.api.redirections;
	const rc = <Result>await common.url.postRequestForm( url, {json} );
	return rc;
}
