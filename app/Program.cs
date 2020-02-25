using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logHelper = new LogHelper();
			logHelper.AddLogMessage( $"{nameof(Main)}: START" );

			logHelper.AddLogMessage( $"{nameof(Main)}: Create the WebHost" );
			var webHost = BuildWebHost( args );

			logHelper.AddLogMessage( $"{nameof(Main)}: Create the service scope" );
			using( var scope = webHost.Services.CreateScope() )
			{
				logHelper.AddLogMessage( $"{nameof(Main)}: Get Startup service" );
				var startup = scope.ServiceProvider.GetRequiredService<Startup>();
				logHelper.AddLogMessage( $"{nameof(Main)}: Launch 'Startup.Initialize()'" );
				startup.Initialize( logHelper, scope.ServiceProvider );
			}

			logHelper.AddLogMessage( $"{nameof(Main)}: Start web host" );
			webHost.Run();

			logHelper.AddLogMessage( $"{nameof(Main)}: EXIT" );
		}

		public static IWebHost BuildWebHost(string[] args) =>
			Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	}
}
