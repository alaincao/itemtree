
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Services
{
	using Regex = System.Text.RegularExpressions.Regex;
	using Cwd = Tree.Cwd;

	public class RedirectionsController : Views.BaseController
	{
		private readonly PageHelper		PageHelper;
		private readonly Cwd			Cwd;
		private readonly Redirections	Redirections;

		public RedirectionsController(PageHelper pageHelper, Cwd cwd, Redirections redirections)
		{
			PageHelper		= pageHelper;
			Cwd				= cwd;
			Redirections	= redirections;
		}

		[HttpPost( Routes.Redirections )]
		public async Task<Utils.TTTServiceResult> Save(string json)
		{
			try
			{
				var logHelper = PageHelper.ScopeLogs;
				if( string.IsNullOrWhiteSpace(json) )
					throw new ArgumentNullException( nameof(json) );

				logHelper.AddLogMessage( $"{nameof(Save)}: START" );
				using( Cwd.PushDisposable(Routes.Redirections) )
				{
					logHelper.AddLogMessage( $"{nameof(Save)}: Check JSON" );
					var entries = await Redirections.GetEntries( logHelper, json:json );

					logHelper.AddLogMessage( $"{nameof(Save)}: Save data" );
					await Cwd.TreeHelper.SetNodeData( Cwd, json );

					logHelper.AddLogMessage( $"{nameof(Save)}: Update cache" );
					Redirections.SetEntries( entries );
				}

				logHelper.AddLogMessage( $"{nameof(Save)}: END" );
				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}
	}

	public class Redirections
	{
		internal class CEntries
		{
			internal class Entry
			{
				public string	Source			{ get; set; }
				public string	Destination		{ get; set; }
				public bool		UseRegex		{ get; set; } = false;
			}
			internal class RegexEntry
			{
				public Regex	Source;
				public Entry	Entry;
			}

			internal IDictionary<string,Entry>	SimpleUrls			{ get; } = new Dictionary<string,Entry>();
			internal List<RegexEntry>			RegularExpressions	{ get; } = new List<RegexEntry>();
		}

		private CEntries		Entries		= null;

		public Redirections()  {}

		private async Task<CEntries> GetEntries(LogHelper logHelper, IServiceProvider services)
		{
			var entries = Entries;
			if( entries != null )
				return entries;

			var cwd = services.GetRequiredService<Tree.Cwd>();
			using( cwd.PushDisposable(Routes.Redirections) )
			{
				entries = await GetEntries( logHelper, nodePath:cwd );
			}
			Entries = entries;
			return entries;
		}

		internal void SetEntries(CEntries entries)
		{
			Entries = entries;
		}

		internal static async Task<CEntries> GetEntries(LogHelper logHelper, Tree.Cwd nodePath=null, string json=null)
		{
			Utils.Assert( (nodePath != null ?1:0) + (json != null ?1:0) == 1, typeof(Redirections), $"One and only one of parameters {nameof(nodePath)} and {nameof(json)} must be specified" );

			if( nodePath != null )
			{
				logHelper.AddLogMessage( $"{nameof(Redirections)}: Retreive JSON from Tree node '{nodePath.Pwd()}'" );
				json = await nodePath.TreeHelper.GetNodeData( nodePath );
				if( json == null )
					json = "[]";
			}

			logHelper.AddLogMessage( $"{nameof(Redirections)}: Parse JSON" );
			var entries = new CEntries();

			var items = json.JSONDeserialize<List<CEntries.Entry>>();
			foreach( var entry in items )
			{
				if( entry.UseRegex )
				{
					entries.RegularExpressions.Add( new CEntries.RegexEntry {
							Source = new Regex( entry.Source.ToLower() ),
							Entry = entry,
						} );
				}
				else
				{
					var src = entry.Source.ToLower();
					if( src.EndsWith("/") && (src != "/") )
						src = src.Substring( 0, src.Length-1 );

					entries.SimpleUrls.Add( src, entry );
					entries.SimpleUrls.Add( src+"/", entry );
				}
			}

			logHelper.AddLogMessage( $"{nameof(Redirections)}: Found {entries.SimpleUrls.Count/2} simple and {entries.RegularExpressions.Count} regex redirections" );
			return entries;
		}

		private async Task<string> GetDestination(LogHelper logHelper, IServiceProvider services, string sourcePath, string sourceQuery)
		{
			var entries = await GetEntries( logHelper, services );
			sourcePath = sourcePath.ToLower();
			string destination;

			// Try simple redirections
			{if( entries.SimpleUrls.TryGetValue(sourcePath, out var entry) )
			{
				logHelper.AddLogMessage( $"{nameof(Redirections)}: Simple redirections match !" );
				destination = entry.Destination;
				goto MATCH;
			}}

			// Try regular expressions
			foreach( var entry in entries.RegularExpressions )
			{
				var match = entry.Source.Match( sourcePath );
				if( ! match.Success )
					continue;
				logHelper.AddLogMessage( $"{nameof(Redirections)}: Regular expressions match !" );

				destination = entry.Entry.Destination;

				var groupsCount = match.Groups.Count;
				if( groupsCount > 1 )
				{
					for( var i=1; i<groupsCount; ++i )
						destination = destination.Replace( "{"+i+"}", match.Groups[i].Value );
				}

				goto MATCH;
			}

			return null;

		MATCH:
			if( ! string.IsNullOrWhiteSpace(sourceQuery) )
			{
				// Append URI parameters
				var ub = new System.UriBuilder( destination );
				var parms = System.Web.HttpUtility.ParseQueryString( ub.Query );
				var newParms = System.Web.HttpUtility.ParseQueryString( sourceQuery );
				foreach( var key in newParms.AllKeys )
					parms[ key ] = newParms[ key ];
				ub.Query = parms.ToString();

				destination = ub.ToString();
			}

			return destination;
		}

		internal static async Task Middleware(HttpContext context, Func<Task> next)
		{
			var pageHelper = context.RequestServices.GetRequiredService<PageHelper>();
			var logHelper = pageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(Redirections)}: START" );

			var self = context.RequestServices.GetRequiredService<Redirections>();
			var sourcePath = context.Request.Path.Value;
			var sourceQuery = context.Request.QueryString.Value;

			var destination = await self.GetDestination( logHelper, context.RequestServices, sourcePath, sourceQuery );
			if( destination != null )
			{
				logHelper.AddLogMessage( $"{nameof(Redirections)}: END ; Redirectig to: {destination}" );
				context.Response.Redirect( destination );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(Redirections)}: END ; No match" );
				await next();
			}
		}
	}
}
