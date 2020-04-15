
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Tree
{
	public static class Middlewares
	{
		private class ReplayRouting
		{
			internal bool		Replaying		= false;
			internal bool		Bypassing		= false;
			internal string		OriginalPath	= null;
			internal Func<Task>	NextPipeline	= null;
		}

		internal static void ConfigureServices(LogHelper initializationLlog, IServiceCollection services)
		{
			initializationLlog.AddLogMessage( $"{nameof(TreeController)+"."+nameof(ConfigureServices)}: Configure Tree's services" );
			services.AddSingleton<TreeHelper>();
			services.AddScoped<ReplayRouting>();
			services.AddScoped<Cwd>();
		}

		internal static Task BeforeRoutingMiddleware(HttpContext context, Func<Task> next)
		{
			var services = context.RequestServices;
			var logHelper = services.GetScopeLog();
			var replay = services.GetRequiredService<ReplayRouting>();
			if( replay.Replaying )
			{
				// If this middleware is reinvoked after replaying, something else is replaying (i.e. more than probably the exception handler)
				// => Let pass through and do nothing in the 'AfterRoutingMiddleWare' either !
				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(BeforeRoutingMiddleware)}: Already replaying => bypassing !" );
				replay.Bypassing = true;
			}
			else
			{
				// Save the pipeline state right before the MVC routing middleware is invoked
				Utils.Assert( replay.Replaying == false, nameof(BeforeRoutingMiddleware), "We are not supposed to be replaying the pipeline at this point" );
				replay.OriginalPath = context.Request.Path;
				replay.NextPipeline = next;
				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(BeforeRoutingMiddleware)}: Saved original path '{replay.OriginalPath}'" );
			}

			return next();
		}

		internal static async Task AfterRoutingMiddleWare(HttpContext context, Func<Task> next)
		{
			var services = context.RequestServices;
			var logHelper = services.GetScopeLog();
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: START" );

			var replay = services.GetRequiredService<ReplayRouting>();
			if( replay.Bypassing )
			{
				// NOOP !
			}
			else if(! replay.Replaying )
			{
				// First time this middleware is invoked
				if( context.GetEndpoint() != null )
					// The MVC router found an endpoint for this request => Continue the pipeline as normal
					goto NEXT;
				// The routing did not find an endpoint

				// Look in the database to see if there's a TreeNode available for this request
				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: Check database" );
				var langInfo = services.GetRequiredService<Language.Info>();
				var cwd = services.GetRequiredService<Cwd>();
				cwd.Cd( langInfo.CleanPath );
				var node = await cwd.TreeHelper.GetNodeMetadata( cwd );
				if( node == null )
					// Nothing found => Continue to 404
					goto NEXT;

				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: Check if a route redirection is available" );
				var route = cwd.TreeHelper.TryGetRouteRedirection( node.B );
				if( route == null )
					// No route for this type of node
					goto NEXT;

				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: *** Replay pipeline using route '{route}'" );
				// nb: using the same principle as the ASP.Net's exception handler
				// cf. https://github.com/dotnet/aspnetcore/blob/3.0/src/Middleware/Diagnostics/src/ExceptionHandler/ExceptionHandlerMiddleware.cs#L107-L127
				context.Request.Path = route;
				// context.Response.Clear();  <== Would clear any cookies previously set (i.e. by the language's middleware)
				var routeValuesFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IRouteValuesFeature>();
				routeValuesFeature?.RouteValues?.Clear();
				context.Response.OnStarting( (state)=>Task.CompletedTask, null );

				replay.Replaying = true;
				await replay.NextPipeline();
				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: END" );
				return;  // nb: DO NOT invoke 'next()'
			}
			else  // Replaying
			{
				var endpointNotFound = (context.GetEndpoint() == null);
				if( endpointNotFound )
					logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: Replaying, but routing still did not find any endpoint" );

				logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: Replaying ; Restore request's original path '{replay.OriginalPath}'" );
				context.Request.Path = replay.OriginalPath;
			}

		NEXT:
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}.{nameof(AfterRoutingMiddleWare)}: NEXT" );
			await next();
		}
	}
}
