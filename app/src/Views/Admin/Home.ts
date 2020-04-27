
import { stringIsNullOrWhitespace } from "../utils";
import * as common from "../common";
import * as loginCtrl from "../../Services/LoginController";
import * as trnCtrl from "../../Services/TranslationController";
import * as trnDto from "../../DTOs/Translation";

const passwordsDontMatchMessage = 'Les 2 mots de passe ne concordent pas';
const passwordChangedMessage = 'Password changed successfully';

export var changePassword : ChangePassword;
export var features : Features;

export async function init(p:{	$passwordBlockingDiv : JQuery,
								$featuresBlockingDiv : JQuery,
							}) : Promise<void>
{
	changePassword	= new ChangePassword( p.$passwordBlockingDiv );
	features		= new Features( p.$featuresBlockingDiv );
}

class ChangePassword
{
	private readonly	$blockingDiv		: JQuery;

	protected readonly	original			: KnockoutObservable<string>;
	protected readonly	newPassword			: KnockoutObservable<string>;
	protected readonly	newPasswordAgain	: KnockoutObservable<string>;

	constructor($blockingDiv:JQuery)
	{
		this.$blockingDiv		= $blockingDiv;
		this.original			= ko.observable( '' );
		this.newPassword		= ko.observable( '' );
		this.newPasswordAgain	= ko.observable( '' );
	}

	protected async change() : Promise<void>
	{
		const self = this;
		const oldPassword = self.original();
		const newPassword = self.newPassword();
		const newPasswordAgain = self.newPasswordAgain();

		if( stringIsNullOrWhitespace(newPassword) )
			return;
		if( stringIsNullOrWhitespace(oldPassword) )
			return;
		if( newPassword != newPasswordAgain )
		{
			common.html.showMessage( passwordsDontMatchMessage );
			return;
		}

		common.html.block( self.$blockingDiv );
		const rc = await loginCtrl.changePassword({ oldPassword, newPassword });
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'change password error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}
		common.html.showMessage( passwordChangedMessage );
		return;
	}
}

abstract class TranslationsBase
{
	private readonly	$blockingDiv	: JQuery;

	private readonly	type			: trnCtrl.TranslationType;
	protected readonly	items			: KnockoutObservableArray<TranslationItem>;
	protected readonly	hasChanges		: KnockoutComputed<boolean>;

	constructor(type:trnCtrl.TranslationType, $blockingDiv:JQuery)
	{
		const self = this;

		this.$blockingDiv	= $blockingDiv;
		this.type			= type;
		this.items			= ko.observableArray();
		this.hasChanges		= ko.computed( ()=>
									{
										const items = self.items();
										for( let i=0; i<items.length; ++i )
										{
											if( items[i].modified() )
												return true;
										}

										return false;
									} );

		this.refresh();
	}

	protected async refresh() : Promise<void>
	{
		const self = this;

		common.html.block( self.$blockingDiv );
		var rc = await trnCtrl.list({ type:self.type });
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'refresh translations error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		const items = rc.translations.map( v=>new TranslationItem(v) );
		self.items( items );
	}

	protected clearLine(index:KnockoutObservable<number>) : void
	{
		const self = this;
		const item = self.items()[ index() ];
		item.setAllLanguagesTo( '' );
	}
	protected addLine() : void
	{
		this.items.push( new TranslationItem() );
	}

	protected async save() : Promise<void>
	{
		const self = this;

		const toSave = self.items()
						.map( (item)=>
							{
								if(! item.modified() )
									return null;

								const dto : trnCtrl.SaveRequestItem = item.toDTO();
								dto.enOriginal = item.enOriginal();
								return dto;
							} )
						.filter( (item)=>(item != null) );

		if( toSave.length == 0 )
		{
			// NOOP => Simply refresh
			await self.refresh();
			return;
		}

		common.html.block( self.$blockingDiv );
		const rc = await trnCtrl.save({ type:self.type, translations:toSave });
		common.html.unblock( self.$blockingDiv );
		if(! rc.success )
		{
			common.utils.error( 'save translations error', { rc } );
			common.html.showMessage( rc.errorMessage );
			return;
		}

		await self.refresh();
	}
}
class TranslationItem extends trnDto.TranslationItemKO
{
	protected readonly showReset	: KnockoutComputed<boolean>;

	constructor(src?:trnDto.TranslationItem)
	{
		super( src );
		const self = this;
		this.showReset = ko.computed( ()=>self.inTranslation() );
	}
}

class Features extends TranslationsBase
{
	constructor($blockingDiv:JQuery)
	{
		super( trnCtrl.TranslationTypes.feature, $blockingDiv );
	}
}
