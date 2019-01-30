
using System;

namespace ItemTTT.Views
{
	public abstract class BaseView : Microsoft.AspNetCore.Mvc.Razor.RazorPage<object>
	{
		private PageHelper		PageHelper;

		/// <summary>Tag parameter to add to scripts URLs so browsers' cache get refreshed</summary>
		protected string		ScriptsTag		{ get { return scriptsTag ?? (scriptsTag = Guid.NewGuid().ToString().Replace("-", "")); } }
		private static string	scriptsTag		= null;

		protected bool	IsEN	{ get; private set; }
		protected bool	IsFR	{ get; private set; }
		protected bool	IsNL	{ get; private set; }

		protected void Init(PageHelper pageHelper)
		{
			PageHelper = pageHelper;

			var logHelper = pageHelper.ScopeLogs;
			logHelper.AddLogMessage( "BaseView Init: START" );

			var lng = pageHelper.CurrentLanguage;
			IsEN = (lng == Languages.en);
			IsFR = (lng == Languages.fr);
			IsNL = (lng == Languages.nl);

			logHelper.AddLogMessage( "BaseView Init: END" );
		}

		protected string Resolve(string route)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), this, $"Missing parameter '{nameof(route)}'" );
			route = route.Replace( Routes.LangParameter, ""+PageHelper.CurrentLanguage );
			return PageHelper.ResolveRoute( route );
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
			var cultureInfo = System.Globalization.CultureInfo.GetCultureInfo( "fr-BE" );
			return price.Value.ToString( "C0", cultureInfo );
		}

		protected string FormatDate(string isoDate)
		{
			var dt = isoDate.FromIsoDate();
			if( dt == null )
				return "";
			return dt.Value.ToString( "dd/MM/yyyy" );
		}

		protected string HtmlShield(string txt)
		{
			txt = HtmlEncoder.Encode( txt );
			txt = txt.Replace( "&#xA;", "<br/>" );  // nb: "&#xA;" => "\n"
			return txt;
		}
	}
}
