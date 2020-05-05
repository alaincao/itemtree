
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Tree
{
	using Metadata = IDictionary<string,object>;
	using NewMetadata = Dictionary<string,object>;

	public class TreeHelper
	{
		private readonly IDictionary<string,string>			RouteRedirections;

		public enum Types
		{
			html,
			translatedHtml,
			image,
			view,

			pageProperty,
		}

		internal readonly string	Root;

		public TreeHelper(Startup startup)
		{
			var logHelper = startup.InitializationLog;
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: START" );

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: Get tree root" );
			Root = startup.Configuration[ AppSettingsKeys.TreeRoot ];
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: {nameof(Root)}: '{Root}'" );

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: Load route handlers" );
			var routeRedirections = new Dictionary<string,string>();
			routeRedirections[""+Types.view]			= Routes.TreeView;
			routeRedirections[""+Types.html]			= Routes.TreeHtml;
			routeRedirections[""+Types.translatedHtml]	= Routes.TreeHtmlTranslated;
			routeRedirections[""+Types.image]			= Routes.TreeImage;
			RouteRedirections = routeRedirections;

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: END" );
		}

		internal async Task<Utils.CustomClass2<Metadata,Models.TreeNode>> GetNode(Cwd cwd)
		{
			Utils.Assert( cwd != null, nameof(GetNode), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var pathDb = cwd.PwdDb();

			logHelper.AddLogMessage( $"{nameof(GetNode)}: Check '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).SingleOrDefaultAsync();
			if( node == null )
				return null;

			Metadata meta;
			if(! node.Meta.StartsWith('{') )
				// Not JSON
				meta = null;
			else
				meta = node.Meta.JSONDeserialize();

			return new Utils.CustomClass2<Metadata, Models.TreeNode>{ A=meta, B=node };
		}
		internal async Task<Utils.CustomClass2<Metadata,Models.TreeNode>> GetNode(Cwd cwd, Types expectedType)
		{
			var node = await GetNode( cwd );
			if( node == null )
				return null;
			if( node.A == null )
				return null;
			if( node.A.TreeMetadata_Type() != ""+expectedType )
				return null;
			return node;
		}

		/** A:Absolute path ; B:Node name */
		public async Task<Utils.CustomClass2<string,string>[]> GetChildNodes(Cwd cwd)
		{
			Utils.Assert( cwd != null, nameof(GetChildNodes), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var pathDb = cwd.PwdDb();

			logHelper.AddLogMessage( $"{nameof(GetChildNodes)}: Retreive child nodes of '{pathDb}'" );
			var paths = await dc.TreeNodes.Where( v=>v.ParentPath == pathDb ).Select( v=>v.Path ).ToArrayAsync();
			var items = paths.Select( (path)=>new Utils.CustomClass2<string,string>{ A=path, B=cwd.GetNodeName(path) } ).ToArray();

			logHelper.AddLogMessage( $"{nameof(GetChildNodes)}: Remove DB's prefix from paths" );
			foreach( var item in items )
				item.A = cwd.Pwd( item.A );

			return items;
		}

		internal async Task<Models.TreeNode> GetOrCreateNode(Cwd cwd, Types? expectedType=null, string data=null)
		{
			Utils.Assert( cwd != null, nameof(GetOrCreateNode), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var pathParentDb = cwd.PwdParentDb();

			logHelper.AddLogMessage( $"{nameof(GetOrCreateNode)}: Check '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).SingleOrDefaultAsync();

			if( node == null )
			{
				logHelper.AddLogMessage( $"{nameof(GetOrCreateNode)}: Check that '{pathParentDb}' exists" );
				var parentExists = await dc.TreeNodes.Where( v=>v.Path == pathParentDb ).Select( v=>(bool?)true ).SingleOrDefaultAsync() ?? false;
				if(! parentExists )
					throw new Utils.TTTException( $"{nameof(TreeHelper)}: Parent of path '{path}' does not exist" );

				logHelper.AddLogMessage( $"{nameof(GetOrCreateNode)}: Create node" );
				string meta;
				if( expectedType == null )
				{
					meta = "";
				}
				else
				{
					var dict = new NewMetadata();
					dict.TreeMetadata_Type( ""+expectedType );
					meta = dict.JSONStringify();
				}
				node = new Models.TreeNode{ Path		= pathDb,
											ParentPath	= pathParentDb,
											Meta		= meta,
											Data		= data??"" };
				dc.TreeNodes.Add( node );
				await dc.SaveChangesAsync();
			}
			else
			{
				if( expectedType != null )
				{
					logHelper.AddLogMessage( $"{nameof(GetOrCreateNode)}: Check node type" );
					if(! node.Meta.StartsWith('{') )
						// Not JSON
						throw new Utils.TTTException( $"{nameof(TreeHelper)}: Unknown node type at '{path}'" );
					var dict = node.Meta.JSONDeserialize();
					if( dict.TreeMetadata_Type() != ""+expectedType )
						throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not of expected type '{expectedType}'" );
				}

				if( data != null )
				{
					logHelper.AddLogMessage( $"{nameof(GetOrCreateNode)}: Update node's data" );
					node.Data = data;
					await dc.SaveChangesAsync();
				}
			}

			return node;
		}

		internal async Task<Utils.CustomClass2<int,Metadata>> GetNodeMetadata(Cwd cwd, Types? expectedType=null)
		{
			Utils.Assert( cwd != null, nameof(GetNodeMetadata), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var dc = cwd.DataContext;

			logHelper.AddLogMessage( $"{nameof(GetNodeMetadata)}: Retreive '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).Select( v=>new{ v.ID, v.Meta } ).SingleOrDefaultAsync();
			if( node == null )
				return null;
			if(! node.Meta.StartsWith('{') )
				// Metadata not JSON
				return null;

			logHelper.AddLogMessage( $"{nameof(GetNodeMetadata)}: Parse node's metadata" );
			var dict = node.Meta.JSONDeserialize();
			if( expectedType != null )
				if( dict.TreeMetadata_Type() != ""+expectedType )
					throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not of expected type '{expectedType}'" );

			return new Utils.CustomClass2<int,Metadata>{ A=node.ID, B=dict };
		}

		internal async Task<string> GetNodeData(Cwd cwd)
		{
			Utils.Assert( cwd != null, nameof(GetNodeData), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var dc = cwd.DataContext;

			logHelper.AddLogMessage( $"{nameof(GetNodeData)}: Retreive '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).Select( v=>new{ v.Data } ).SingleOrDefaultAsync();
			if( node == null )
				return null;

			return node.Data;
		}

		internal async Task SetNodeData(Cwd cwd, string data, Types? expectedType=null)
		{
			Utils.Assert( cwd != null, nameof(SetNodeData), $"Missing parameter '{nameof(cwd)}'" );
			Utils.Assert( data != null, nameof(SetNodeData), $"Missing parameter '{nameof(data)}'" );
			var logHelper = cwd.LogHelper;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var dc = cwd.DataContext;

			logHelper.AddLogMessage( $"{nameof(SetNodeData)}: Retreive '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).SingleOrDefaultAsync();
			if( node == null )
				throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not found" );

			if( expectedType != null )
			{
				var dict = node.Meta.JSONDeserialize();
				if( dict.TreeMetadata_Type() != ""+expectedType )
					throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not of expected type '{expectedType}'" );
			}

			logHelper.AddLogMessage( $"{nameof(SetNodeData)}: Update node's data" );
			node.Data = data;
			await dc.SaveChangesAsync();
		}

		internal async Task DelTree(Cwd cwd, bool included=true)
		{
			Utils.Assert( cwd != null, nameof(DelTree), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var pathDb = cwd.PwdDb();

			logHelper.AddLogMessage( $"{nameof(DelTree)}: START: '{pathDb}' ; included:{included}" );

			var ids = await dc.TreeNodes.Where( v=>v.Path.StartsWith(pathDb+Cwd.Separator) ).Select( v=>v.ID ).ToListAsync();
			if( included )
			{
				var id = await dc.TreeNodes.Where( v=>v.Path == pathDb ).Select( v=>(int?)v.ID ).SingleOrDefaultAsync();
				if( id != null )
					ids.Add( id.Value );
			}

			logHelper.AddLogMessage( $"{nameof(DelTree)}: Remove {ids.Count} nodes" );
			foreach( var id in ids )
			{
				var node = new Models.TreeNode{ ID=id };
				dc.TreeNodes.Attach( node );
				dc.TreeNodes.Remove( node );
			}
			await dc.SaveChangesAsync();
		}

		internal static async IAsyncEnumerable<string> SaveTree(Cwd cwd)
		{
			Utils.Assert( cwd != null, nameof(SaveTree), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var path = cwd.PwdDb();
			logHelper.AddLogMessage( $"{nameof(SaveTree)}: START: '{path}'" );

			var pathBase = path + Cwd.Separator;
			var q = dc.TreeNodes.Where( v=> (v.Path == path) || v.Path.StartsWith(pathBase) )
								.OrderBy( v=>v.Path )
								.Select( v=>new{ v.Path, v.Meta, v.Data } );
			await foreach( var node in q.AsAsyncEnumerable() )
			{
				var nodePath = node.Path.Substring( path.Length );
				if( nodePath.Length == 0 )
					// Base path => root
					nodePath = Cwd.Separator;
				logHelper.AddLogMessage( $"{nameof(SaveTree)}: At '{nodePath}'" );
				var line = (new{ Path=nodePath, node.Meta, node.Data }).JSONStringify();
				yield return line;
			}
		}

		internal string TryGetRouteRedirection(Metadata metaData)
		{
			var nodeType = metaData.TreeMetadata_Type();
			return RouteRedirections.TryGet( nodeType );
		}
	}

	internal static partial class ExtensionMethods
	{
		internal static string TreeMetadata_Type(this Metadata metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_Type), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "type" );
		}
		internal static void TreeMetadata_Type(this Metadata metadata, string value)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_Type), $"Missing parameter '{nameof(metadata)}'" );
			if( value == null )
				metadata.Remove( "type" );
			else
				metadata[ "type" ] = value;
		}
	}
}
