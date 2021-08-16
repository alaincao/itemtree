
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Views
{
	public abstract class BaseView : Microsoft.AspNetCore.Mvc.Razor.RazorPage<object>
	{
		private PageHelper			PageHelper;
		private LogHelper			LogHelper;
		protected Tree.Cwd			Cwd			{ get; private set; }
		protected Tree.TreeHelper	TreeHelper	{ get; private set; }

		/// <summary>Tag parameter to add to scripts URLs so browsers' cache get refreshed</summary>
		protected string		ScriptsTag		{ get { return scriptsTag ?? (scriptsTag = Guid.NewGuid().ToString().Replace("-", "")); } }
		private static string	scriptsTag		= null;

		protected bool	IsAuthenticated		=> PageHelper.IsAuthenticated;

		protected bool	IsEN	{ get; private set; }
		protected bool	IsFR	{ get; private set; }
		protected bool	IsNL	{ get; private set; }

		protected bool	UseMini	{ get; private set; }
		protected bool	UseMaxi	{ get; private set; }

		protected void Init(PageHelper pageHelper)
		{
			pageHelper.ScopeLogs.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: START" );

			PageHelper	= pageHelper;
			LogHelper	= pageHelper.ScopeLogs;
			Cwd			= Context.RequestServices.GetRequiredService<Tree.Cwd>();
			TreeHelper	= Cwd.TreeHelper;

			if( string.IsNullOrWhiteSpace(pageHelper.Parameters.PageTitle) )
			{
				LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: Use default page title" );
				var startup = this.Context.RequestServices.GetRequiredService<Startup>();
				pageHelper.Parameters.PageTitle = startup.Configuration[ AppSettingsKeys.DefaultTitle ];
			}

			var lng = pageHelper.CurrentLanguage;
			IsEN = (lng == Languages.en);
			IsFR = (lng == Languages.fr);
			IsNL = (lng == Languages.nl);

			UseMini = ( Utils.IsDebug == false );
			UseMaxi = (! UseMini);

			LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Init)}: END" );
		}

		protected string Resolve(string route, bool full=false, Languages? lang=null)
		{
			if( route == "" )
				route = "~";
			else if( route == null )
				Utils.Fail( this, $"Missing parameter '{nameof(route)}'" );

			if( lang == null )
				lang = PageHelper.CurrentLanguage;

			route = route.Replace( Routes.LangParameter, Language.ToUrlValue[lang.Value] );
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

		protected string Tree_Pwd(string path=null, bool full=false, Languages? languagePrefix=null)
		{
			string pwd;
			if( path == null )
			{
				pwd = Cwd.Pwd();
			}
			else
			{
				using( Cwd.PushDisposable(path) )
					pwd = Cwd.Pwd();
			}

			var langStr = "";
			if( languagePrefix != null )
				langStr = $"/{Language.ToUrlValue[languagePrefix.Value]}";

			if( full )
				pwd = $"{Context.Request.Scheme}://{Context.Request.Host}{langStr}{pwd}";

			return pwd;
		}

		protected string Tree_SanitizeName(string str)
		{
			return ItemTTT.Tree.Cwd.SanitizeName( str );
		}
		protected string Tree_SanitizePath(string str)
		{
			return ItemTTT.Tree.Cwd.SanitizePath( str );
		}

		protected string Tree_GetNodeName(string path=null)
		{
			return Cwd.GetNodeName( path );
		}

		protected async Task<string[]> Tree_GetChildNodeNames(string path=null)
		{
			LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Tree_GetChildNodeNames)}: '{path??Cwd.Pwd()}'" );
			using( (path == null) ? null : Cwd.PushDisposable(path) )
			{
				return ( await TreeHelper.GetChildNodes(Cwd) ).Select( v=>
					{
						if( path == null )
							// Use relative path
							return v.B;
						else
							// Use absolute path
							return v.A;
					} ).ToArray();
			}
		}

		protected async Task<IDictionary<string,object>> Tree_GetNodeMetaData(string path=null)
		{
			LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Tree_GetNodeMetaData)}: '{path??Cwd.Pwd()}'" );
			using( (path == null) ? null : Cwd.PushDisposable(path) )
			{
				(await TreeHelper.GetNodeMetaData( Cwd )).R( out var _, out var meta );
				return meta;
			}
		}

		protected async Task<dynamic> Tree_GetNodeData(string path=null)
		{
			LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Tree_GetNodeData)}: '{path??Cwd.Pwd()}'" );
			using( (path == null) ? null : Cwd.PushDisposable(path) )
			{
				var json = await TreeHelper.GetNodeData( Cwd );
				if( string.IsNullOrWhiteSpace(json) )
				{
					LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Tree_GetNodeData)}: Data not found" );
					json = "{}";
				}
				return json.JSONDeserialize<object>();
			}
		}

		protected async Task<string> Tree_GetTranslatedContent(string path=null, Languages? lang=null)
		{
			LogHelper.AddLogMessage( $"{nameof(BaseView)}.{nameof(Tree_GetTranslatedContent)}: '{path??Cwd.Pwd()}'" );
			using( (path == null) ? null : Cwd.PushDisposable(path) )
			{
				var json = await Cwd.TreeHelper.GetNodeData( Cwd );
				return string.IsNullOrWhiteSpace(json) ? "" : Tree.TreeController.GetTranslatedNodeText( PageHelper, json, lang:lang );
			}
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
