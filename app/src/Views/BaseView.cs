
using System;

using Microsoft.Extensions.DependencyInjection;

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

		protected bool	UseMini	{ get; private set; }
		protected bool	UseMaxi	{ get; private set; }

		protected void Init(PageHelper pageHelper)
		{
			PageHelper = pageHelper;

			var logHelper = pageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: START" );

			if( string.IsNullOrWhiteSpace(pageHelper.Parameters.PageTitle) )
			{
				logHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: START" );
				var startup = this.Context.RequestServices.GetRequiredService<Startup>();
				pageHelper.Parameters.PageTitle = startup.Configuration[ AppSettingsKeys.DefaultTitle ];
			}

			var lng = pageHelper.CurrentLanguage;
			IsEN = (lng == Languages.en);
			IsFR = (lng == Languages.fr);
			IsNL = (lng == Languages.nl);

			UseMini = ( Utils.IsDebug == false );
			UseMaxi = (! UseMini);

			logHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: END" );
		}

		protected string Resolve(string route, bool full=false)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), this, $"Missing parameter '{nameof(route)}'" );
			route = route.Replace( Routes.LangParameter, ""+PageHelper.CurrentLanguage );
			route = PageHelper.ResolveRoute( route );
			if( full )
				route = $"{Context.Request.Scheme}://{Context.Request.Host}{route}";
			return route;
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
