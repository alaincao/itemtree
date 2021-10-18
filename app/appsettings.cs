
namespace ItemTTT
{
	internal static class AppSettingsKeys
	{
		internal const string	FileName			= "appsettings.json";
		internal const string	TreeRoot			= "TreeRoot";
		internal const string	VirtualDirectory	= "VirtualDirectory";

		internal const string DefaultTitle		= "DefaultTitle";
		internal static class ConnectionStrings
		{
			internal const string		ItemTTT			= "ConnectionStrings:ItemTTT";
		}
		internal class OAuthSchemes
		{
			internal bool		AllowAlLUsers;
			internal string		AuthorizationEndpoint;
			internal string		TokenEndpoint;
			internal string		ClientId;
			internal string		ClientSecret;
			internal string[]	Scope						= null;
			internal string		SigningCallbackPath;
			internal string		UserInformationEndpoint;
			internal string		UserClaimField;
		}
	}
}
