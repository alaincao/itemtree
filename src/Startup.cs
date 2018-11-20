﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ItemTTT
{
	public class Startup
	{
		internal LogHelper						InitializationLog;
		internal readonly IConfigurationRoot	Configuration;
		internal static string					ContentRootPath		{ get; private set; }

		public Startup(IHostingEnvironment env)
		{
			InitializationLog = new LogHelper();

			// Prepare configuration for appsettings[.Development].json
			Configuration = CreateConfiguration( InitializationLog, rootPath:env.ContentRootPath, environmentName:env.EnvironmentName );
		}

		internal static IConfigurationRoot CreateConfiguration(LogHelper logHelper, string rootPath, string environmentName)
		{
			logHelper.AddLogMessage( "Startup: CreateConfiguration START" );

			Utils.Assert( !string.IsNullOrWhiteSpace(rootPath), typeof(Startup), "Missing parameter rootPath" );
			var builder = new ConfigurationBuilder();
			builder.SetBasePath( rootPath );
			builder.AddJsonFile( AppSettingsKeys.FileName, optional:false, reloadOnChange:true );
			if( environmentName != null )
				builder.AddJsonFile( $"appsettings.{environmentName}.json", optional:true );
			builder.AddEnvironmentVariables();
			var rc = builder.Build();

			logHelper.AddLogMessage( "Startup: CreateConfiguration END" );
			return rc;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			InitializationLog.AddLogMessage( "Startup: ConfigureServices START" );

			InitializationLog.AddLogMessage( "Startup: ConfigureServices: Add this instance to the services" );
			services.AddSingleton<Startup>( v=>this );

			InitializationLog.AddLogMessage( "Startup: ConfigureServices: Setup MVC" );
			services.AddMvc();

			InitializationLog.AddLogMessage( "Startup: ConfigureServices: Add database service" );
			var connectionString = Configuration[ AppSettingsKeys.ConnectionStrings.ItemTTT ];
			services.AddDbContext<Models.ItemTTT>( options=>
				{
					options.UseSqlServer( connectionString );
				} );

			InitializationLog.AddLogMessage( "Startup: ConfigureServices: Add custom services" );

			services.AddScoped<PageHelper>();
			// NB: This is required for the PageHelper to be able to use the UrlHelper (https://stackoverflow.com/questions/30696337/unable-to-utilize-urlhelper)
			services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor,Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();

			InitializationLog.AddLogMessage( "Startup: ConfigureServices END" );
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			InitializationLog.AddLogMessage( "Startup: Configure START" );

			if( env.IsDevelopment() )
			{
				Utils.IsDebug = true;
				app.UseDeveloperExceptionPage();  // TODO: ACA: Exception pages
			}

			ContentRootPath = env.ContentRootPath;

			// Support for docker reverse-proxy & https
			// c.f. https://docs.microsoft.com/en-us/aspnet/core/publishing/linuxproduction?tabs=aspnetcore2x
			// & https://github.com/IdentityServer/IdentityServer4/issues/1331
			{
				var options = new ForwardedHeadersOptions
					{
						ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
					};
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();
				app.UseForwardedHeaders( options );
			}

			app.UseStaticFiles();

			app.UseAuthentication();

			app.UseMvc();

			InitializationLog.AddLogMessage( "Startup: Configure END" );
		}

		internal void Initialize(LogHelper logHelper, IServiceProvider initializationServices)
		{
			logHelper.AddLogMessage( "Startup: Initialize START" );

			logHelper.AddLogMessage( "Startup: Initialize: Replace 'InitializationLog'" );
			logHelper.Merge( InitializationLog );
			InitializationLog = logHelper;

			logHelper.AddLogMessage( "Startup: Initialize END" );
		}
	}

	// GET '/initialization-log'
	public class InitializationLog : Microsoft.AspNetCore.Mvc.Controller
	{
		private readonly Startup	Startup;

		public InitializationLog(Startup startup)
		{
			Startup = startup;
		}

		[HttpGet( Routes.InitializationLog )]
		public string Get()
		{
			return Startup.InitializationLog.GetLogs();
		}
	}
}
