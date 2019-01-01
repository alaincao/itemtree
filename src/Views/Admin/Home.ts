
import * as common from "../common";
import * as loginCtrl from "../../Services/LoginController";

const passwordsDontMatchMessage = 'Les 2 mots de passe ne concordent pas';
const passwordChangedMessage = 'Password changed successfully';

export var changePassword : ChangePassword;

export async function init(p:{ })
{
	changePassword = new ChangePassword();
}

class ChangePassword
{
	protected	original			: KnockoutObservable<string>;
	protected	newPassword			: KnockoutObservable<string>;
	protected	newPasswordAgain	: KnockoutObservable<string>;

	constructor()
	{
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

		if( common.utils.stringIsNullOrWhitespace(newPassword) )
			return;
		if( common.utils.stringIsNullOrWhitespace(oldPassword) )
			return;
		if( newPassword != newPasswordAgain )
		{
			common.html.showMessage( passwordsDontMatchMessage );
			return;
		}

		const rc = await loginCtrl.changePassword({ oldPassword, newPassword });
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
