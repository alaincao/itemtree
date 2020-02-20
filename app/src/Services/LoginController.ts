
import * as common from "../Views/common";
import Result from "../Utils/TTTServiceResult";

export async function changePassword(p:{ oldPassword:string, newPassword:string }) : Promise<Result>
{
	const url = common.routes.api.changePassword;
	const rc = <Result>await common.url.postRequestForm( url, p );
	return rc;
}
