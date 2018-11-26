
namespace ItemTTT.Views
{
	public abstract class BaseView : Microsoft.AspNetCore.Mvc.Razor.RazorPage<object>
	{
		protected bool	IsEN	{ get; private set; }
		protected bool	IsFR	{ get; private set; }
		protected bool	IsNL	{ get; private set; }

		protected void Init(PageHelper pageHelper)
		{
			pageHelper.ScopeLogs.AddLogMessage( "BaseView Init: START" );

			var lng = pageHelper.CurrentLanguage;
			IsEN = (lng == Languages.en);
			IsFR = (lng == Languages.fr);
			IsNL = (lng == Languages.nl);

			pageHelper.ScopeLogs.AddLogMessage( "BaseView Init: END" );
		}

		protected string JSON(object obj, bool indented=false)
		{
			return obj.JSONStringify( indented:indented );
		}
	}
}
