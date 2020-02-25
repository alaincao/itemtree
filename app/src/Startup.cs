using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ItemTTT
{
	public class Startup
	{
		internal const string 	AuthScheme 		= Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
		internal const string	ViewsLocation	= "/src/Views/";

		internal LogHelper						InitializationLog;
		internal readonly IConfigurationRoot	Configuration;
		internal static string					ContentRootPath		{ get; private set; }

		public Startup(IWebHostEnvironment env)
		{
			InitializationLog = new LogHelper();

			// Prepare configuration for appsettings[.Development].json
			Configuration = CreateConfiguration( InitializationLog, rootPath:env.ContentRootPath, environmentName:env.EnvironmentName );
		}

		internal static IConfigurationRoot CreateConfiguration(LogHelper logHelper, string rootPath, string environmentName)
		{
			logHelper.AddLogMessage( $"{nameof(Startup)}: CreateConfiguration START" );

			Utils.Assert( !string.IsNullOrWhiteSpace(rootPath), typeof(Startup), "Missing parameter rootPath" );
			var builder = new ConfigurationBuilder();
			builder.SetBasePath( rootPath );
			builder.AddJsonFile( AppSettingsKeys.FileName, optional:false, reloadOnChange:true );
			if( environmentName != null )
				builder.AddJsonFile( $"appsettings.{environmentName}.json", optional:true );
			builder.AddEnvironmentVariables();
			var rc = builder.Build();

			logHelper.AddLogMessage( $"{nameof(Startup)}: CreateConfiguration END" );
			return rc;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices START" );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Add this instance to the services" );
			services.AddSingleton<Startup>( v=>this );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Register cookie authentication" );
			var authentication = services.AddAuthentication( AuthScheme );
			authentication.AddCookie( AuthScheme );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Setup MVC" );
			services.AddRouting();
			services.AddControllers();
			services.AddRazorPages();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Configure Views location" );
			// https://stackoverflow.com/questions/36747293/how-to-specify-the-view-location-in-asp-net-core-mvc-when-using-custom-locations
			services.Configure<RazorViewEngineOptions>( o=>
				{
					// {2} is area, {1} is controller,{0} is the action    
					o.ViewLocationFormats.Add( ViewsLocation+"{1}/{0}"+RazorViewEngine.ViewExtension );
					o.ViewLocationFormats.Add( ViewsLocation+"Shared/{0}"+RazorViewEngine.ViewExtension );
				} );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Add languages constraint" );
			services.Configure<RouteOptions>( opt=>opt.ConstraintMap.Add(Language.ConstraintName, typeof(LanguageRouteConstraint)) );
 
			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Add database service" );
			var connectionString = Configuration[ AppSettingsKeys.ConnectionStrings.ItemTTT ];
			services.AddDbContext<Models.ItemTTTContext>( options=>
				{
					options.UseSqlServer( connectionString );
				} );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices: Add custom services" );

			services.AddScoped<PageHelper>();
			// NB: This is required for the PageHelper to be able to use the UrlHelper (https://stackoverflow.com/questions/30696337/unable-to-utilize-urlhelper)
			services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor,Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();

			services.AddScoped<Views.Shared.LayoutHelper>();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: ConfigureServices END" );
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure START" );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Add handler for internal errors (i.e. status 500)" );
			if( env.IsDevelopment() )
			{
				Utils.IsDebug = true;
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler( Routes.Error );
			}
			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Add handler for other errors (e.g. 404)" );
			app.UseStatusCodePagesWithReExecute( Routes.ErrorStatus.Replace("{status}", "{0}") );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Content root path: '{env.ContentRootPath}'" );
			ContentRootPath = env.ContentRootPath;

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Support for docker reverse-proxy & https" );
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

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Add static files support" );
			app.UseStaticFiles();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Activate authentication" );
			app.UseAuthentication();
			app.UseCookiePolicy();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Add MVC support" );
			app.UseRouting();
			app.UseEndpoints( endpoints=>
				{
					endpoints.MapControllers();
				} );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure: Add languages redirections" );
			app.UseLanguageRedirect();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}: Configure END" );
		}

		internal void Initialize(LogHelper logHelper, IServiceProvider initializationServices)
		{
			logHelper.AddLogMessage( $"{nameof(Startup)}: Initialize START" );

			logHelper.AddLogMessage( $"{nameof(Startup)}: Initialize: Replace 'InitializationLog'" );
			logHelper.Merge( InitializationLog );
			InitializationLog = logHelper;

			logHelper.AddLogMessage( $"{nameof(Startup)}: Initialize END" );
		}
	}

	// GET '/initialization-log'
	public class InitializationLog : Views.BaseController
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
