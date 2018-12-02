
namespace ItemTTT.Views
{
	public abstract class BaseView : Microsoft.AspNetCore.Mvc.Razor.RazorPage<object>
	{
		protected bool	IsEN	{ get; private set; }
		protected bool	IsFR	{ get; private set; }
		protected bool	IsNL	{ get; private set; }

		protected void Init(PageHelper pageHelper)
		{
			var logHelper = pageHelper.ScopeLogs;
			logHelper.AddLogMessage( "BaseView Init: START" );

			var lng = pageHelper.CurrentLanguage;
			IsEN = (lng == Languages.en);
			IsFR = (lng == Languages.fr);
			IsNL = (lng == Languages.nl);

			logHelper.AddLogMessage( "BaseView Init: END" );
			if( Utils.IsDebug )
				// Add scope's log to page parameters
				pageHelper.Parameters["Logs"] = logHelper.GetLogLines();
		}

		protected string JSON(object obj, bool? indented=null)
		{
			if( indented == null )
				indented = ( Utils.IsDebug ? true : false );
			return obj.JSONStringify( indented:indented.Value );
		}

		protected string FormatPrice(int? price)
		{
			if( price == null )
				return "-";
			return price.Value.ToString("C0");
		}
	}
}
