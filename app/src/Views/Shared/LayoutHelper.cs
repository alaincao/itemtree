
using System.Linq;
using System.Threading.Tasks;

namespace ItemTTT.Views.Shared
{
	public class LayoutHelper
	{
		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;
		private LogHelper						LogHelper		=> PageHelper.ScopeLogs;

		public LayoutHelper(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;

			LogHelper.AddLogMessage( $"LayoutHelper: Constructor" );
		}

		public string GetPageParametersJSON()
		{
			var logHelper = LogHelper;
			logHelper.AddLogMessage( $"LayoutHelper.GetPageParametersJSON: START" );
			var parameters = PageHelper.Parameters;

			logHelper.AddLogMessage( $"LayoutHelper.GetPageParametersJSON: Set 'HasErrors'" );
			parameters.HasErrors = logHelper.HasErrors;
			if( Utils.IsDebug || logHelper.HasErrors )
			{
				logHelper.AddLogMessage( $"LayoutHelper.GetPageParametersJSON: Add scope's log to page parameters" );
				parameters.Logs = logHelper.GetLogLines();
			}

			logHelper.AddLogMessage( $"LayoutHelper.GetPageParametersJSON: Create JSON" );
			var indented = ( Utils.IsDebug ? true : false );
			var json = parameters.JSONStringify( indented:indented );

			logHelper.AddLogMessage( $"LayoutHelper.GetPageParametersJSON: END" );
			return json;
		}
	}
}
