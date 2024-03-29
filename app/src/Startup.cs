﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
	using OAuthScheme = AppSettingsKeys.OAuthSchemes;

	public class Startup
	{
		internal const string 	CookieAuthenticationScheme 	= Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
		internal const string	ViewsLocation				= "/src/Views/";

		internal LogHelper							InitializationLog;
		internal readonly IConfigurationRoot		Configuration;
		internal static string						ContentRootPath		{ get; private set; }
		internal string								ConnectionString	{ get; private set; }
		internal IDictionary<string,OAuthScheme>	OAuthSchemes		{ get; private set; }

		public Startup(IWebHostEnvironment env)
		{
			var logHelper = new LogHelper();
			logHelper.AddLogMessage( $"{nameof(Startup)}: Constructor START" );

			InitializationLog = logHelper;

			if( env.IsDevelopment() )
				Utils.IsDebug = true;
			logHelper.AddLogMessage( $"{nameof(Startup)}: {nameof(Utils.IsDebug)}: '{Utils.IsDebug}'" );

			// Prepare configuration for appsettings[.Development].json
			Configuration = CreateConfiguration( InitializationLog, rootPath:env.ContentRootPath, environmentName:env.EnvironmentName );

			logHelper.AddLogMessage( $"{nameof(Startup)}: Constructor END" );
		}

		internal static IConfigurationRoot CreateConfiguration(LogHelper logHelper, string rootPath, string environmentName)
		{
			logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(CreateConfiguration)} START" );

			Utils.Assert( !string.IsNullOrWhiteSpace(rootPath), typeof(Startup), "Missing parameter rootPath" );
			var builder = new ConfigurationBuilder();
			builder.SetBasePath( rootPath );
			builder.AddJsonFile( AppSettingsKeys.FileName, optional:false, reloadOnChange:true );
			if( environmentName != null )
				builder.AddJsonFile( $"appsettings.{environmentName}.json", optional:true );
			builder.AddEnvironmentVariables();
			var rc = builder.Build();

			logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(CreateConfiguration)} END" );
			return rc;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)} START" );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Add this instance to the services" );
			services.AddSingleton<Startup>( v=>this );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Register cookie authentication" );
			var authentication = services.AddAuthentication( options=>
				{
					options.DefaultScheme			= CookieAuthenticationScheme;
					options.DefaultChallengeScheme	= CookieAuthenticationScheme;
				} );
			authentication.AddCookie( CookieAuthenticationScheme, options=>
				{
					options.LoginPath	= Routes.Login;
					options.LogoutPath	= Routes.Logout;
				} );

			OAuthSchemes = Services.OAuthHandler.GetSchemes( InitializationLog, Configuration );
			foreach( var pair in OAuthSchemes )
			{
				var schemeName = pair.Key;
				var scheme = pair.Value;
				InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Add OAuth scheme '{schemeName}'" );
				authentication.AddOAuth<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions,Services.OAuthHandler>( authenticationScheme:schemeName, displayName:$"OAuth - {schemeName}", configureOptions:(options)=>
					{
						options.ClaimsIssuer			= schemeName;
						options.AuthorizationEndpoint	= scheme.AuthorizationEndpoint;
						options.TokenEndpoint			= scheme.TokenEndpoint;
						options.ClientId				= scheme.ClientId;
						options.ClientSecret			= scheme.ClientSecret;
						options.CallbackPath			= scheme.SigningCallbackPath;
						options.UserInformationEndpoint = scheme.UserInformationEndpoint;
						foreach( var scope in scheme.Scope??new string[]{} )
							options.Scope.Add( scope );

						// cf. https://github.com/IdentityServer/IdentityServer4/issues/4165#issuecomment-599146559
						options.CorrelationCookie.SameSite		= SameSiteMode.None;
						options.CorrelationCookie.SecurePolicy	= CookieSecurePolicy.Always;
						options.CorrelationCookie.IsEssential	= true;
					} );
			}

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Setup routing & MVC" );
			services.AddRouting();
			services.AddControllers();
			services.AddRazorPages().AddRazorRuntimeCompilation();

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Configure Views location" );
			// https://stackoverflow.com/questions/36747293/how-to-specify-the-view-location-in-asp-net-core-mvc-when-using-custom-locations
			services.Configure<RazorViewEngineOptions>( o=>
				{
					// {2} is area, {1} is controller,{0} is the action
					o.ViewLocationFormats.Add( ViewsLocation+"{1}/{0}"+RazorViewEngine.ViewExtension );
					o.ViewLocationFormats.Add( ViewsLocation+"Shared/{0}"+RazorViewEngine.ViewExtension );
				} );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Add languages constraint" );
			services.Configure<RouteOptions>( opt=>opt.ConstraintMap.Add(Language.ConstraintName, typeof(LanguageRouteConstraint)) );
 
			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Add database service" );
			ConnectionString = Configuration[ AppSettingsKeys.ConnectionStrings.ItemTTT ];
			services.AddDbContext<Models.ItemTTTContext>( options=>
				{
					options.UseSqlServer( ConnectionString );
				} );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)}: Add custom services" );

			services.AddScopedLogHelper();
			services.AddScopedHttpContextReference();
			services.AddScoped<Language.Info>();
			services.AddScoped<PageHelper>();
			services.AddSingleton<Services.Redirections>();
			services.AddScoped<Views.Shared.LayoutHelper>();
			Tree.Middlewares.ConfigureServices( InitializationLog, services );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(ConfigureServices)} END" );
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)} START" );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Add handler for internal errors (i.e. status 500)" );
			if( Utils.IsDebug )
				app.UseDeveloperExceptionPage();
			else
				app.UseExceptionHandler( Routes.Error );
			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Add handler for other errors (e.g. 404)" );
			app.UseStatusCodePagesWithReExecute( Routes.ErrorStatus.Replace("{status}", "{0}") );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Content root path: '{env.ContentRootPath}'" );
			ContentRootPath = env.ContentRootPath;

			var virtualDirectory = Configuration[ AppSettingsKeys.VirtualDirectory ];
			if(! string.IsNullOrWhiteSpace(virtualDirectory) )
			{
				InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Using virtual directory '{virtualDirectory}'" );
				app.UsePathBase( virtualDirectory );
			}

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Support for docker reverse-proxy & https" );
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

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)}: Configure the content's pipeline" );
			app.UseStaticFiles( new StaticFileOptions
				{
					ServeUnknownFileTypes = true,
				} );

			app.UseAuthentication();
			app.UseCookiePolicy();

			app.UseSaveHttpContext();
			app.Use( Language.LanguageInfoMiddleware );
			app.Use( Tree.Middlewares.BeforeRoutingMiddleware );
			app.UseRouting();
			app.Use( Tree.Middlewares.AfterRoutingMiddleWare );
			app.UseEndpoints( endpoints=>endpoints.MapControllers() );
			app.Use( Services.Redirections.Middleware );
			app.Use( Language.LanguageRedirectionMiddleware );

			InitializationLog.AddLogMessage( $"{nameof(Startup)}.{nameof(Configure)} END" );
		}

		internal async Task Initialize(LogHelper logHelper, IServiceProvider initializationServices)
		{
			logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(Initialize)} START" );

			logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(Initialize)}: Replace 'InitializationLog'" );
			logHelper.Merge( InitializationLog );
			InitializationLog = logHelper;

			foreach( var suffix in new string[]{ null, Models.TreeNode.RootUsersSuffix } )
			{
				logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(Initialize)}: Check tree root '{suffix}'" );
				var cwd = Tree.Cwd.New( logHelper, initializationServices, rootSuffix:suffix );
				await cwd.EnsureRootFolder( rootSuffix:suffix );
			}

			await Services.LoginController.Initialize( logHelper, initializationServices );

			logHelper.AddLogMessage( $"{nameof(Startup)}.{nameof(Initialize)} END" );
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

	public static partial class ExtensionMethods
	{
		internal static void AddScopedLogHelper(this IServiceCollection services)
		{
			services.AddScoped<LogHelper>();
		}
		internal static LogHelper GetScopeLog(this IServiceProvider services)
		{
			return services.GetRequiredService<LogHelper>();
		}
	}

	public static partial class ExtensionMethods
	{
		internal static void AddScopedHttpContextReference(this IServiceCollection services)
		{
			services.AddScoped<Utils.CustomClass1<HttpContext>>();
		}
		internal static void UseSaveHttpContext(this IApplicationBuilder app)
		{
			app.Use( (context,next)=>
				{
					var services = context.RequestServices;
					var logHelper = services.GetScopeLog();
					logHelper.AddLogMessage( $"{nameof(UseSaveHttpContext)}: *** Pipeline start ; Save HttpContext reference"); // nb: This is our first middleware
					var reference = services.GetRequiredService<Utils.CustomClass1<Microsoft.AspNetCore.Http.HttpContext>>();
					reference.A = context;

					return next();
				} );
		}
		internal static HttpContext GetHttpContext(this IServiceProvider services)
		{
			return services.GetRequiredService<Utils.CustomClass1<HttpContext>>().A;
		}
	}
}
