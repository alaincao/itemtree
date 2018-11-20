using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ItemTTT
{
	// NB: Scoped service
	public class PageHelper
	{
		public class PageParameters : Dictionary<string,object>
		{
			internal PageParameters()
			{
				PageTitle = "";
			}
			public string	PageTitle	{ get { return (string)this["PageTitle"]; } set { this["PageTitle"] = value; } }
		}

		internal LogHelper				ScopeLogs			{ get; private set; } = new LogHelper();
		public PageParameters			Parameters			{ get; private set; } = new PageParameters();
		internal Func<string,string>	UrlHelperContent	{ get; private set; } = null;

		public bool		UseMinified		{ get { return Utils.IsDebug == false; } }

		public PageHelper(IActionContextAccessor contextAccessor)
		{
			ScopeLogs.AddLogMessage( "PageHelper: Constructor" );

			// Create 'UrlHelperContent' callback
			var urlHelper = (UrlHelper)null;
			UrlHelperContent = (route)=>
				{
					if( urlHelper == null )
						urlHelper = new UrlHelper( contextAccessor.ActionContext );
					return urlHelper.Content( route );
				};
		}

		internal string ResolveRoute(string route)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(route), GetType(), "Missing parameter 'route'" );
			if( route[0] == '/' )
				route = '~'+route;
			return UrlHelperContent( route );
		}
	}
}
