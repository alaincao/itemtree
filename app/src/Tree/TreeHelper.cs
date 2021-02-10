
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Tree
{
	using MetaData = IDictionary<string,object>;
	using NewMetadata = Dictionary<string,object>;

	public class TreeHelper
	{
		private readonly IDictionary<string,string>			RouteRedirections;

		public enum Types
		{
			html,
			translatedHtml,
			file,
			image,
			view,

			pageProperty,
		}

		internal readonly string	Root;
		private readonly string		ConnectionString;

		public TreeHelper(Startup startup) : this(	logHelper			: startup.InitializationLog,
													connectionString	: startup.ConnectionString,
													root				: startup.Configuration[ AppSettingsKeys.TreeRoot ] )
		{
		}

		internal TreeHelper(LogHelper logHelper, string connectionString, string root)
		{
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: START" );
			ConnectionString = connectionString;

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: Get tree root" );
			Root = root;
			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: {nameof(Root)}: '{Root}'" );

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: Load route handlers" );
			var routeRedirections = new Dictionary<string,string>();
			routeRedirections[""+Types.view]			= Routes.TreeView;
			routeRedirections[""+Types.html]			= Routes.TreeHtml;
			routeRedirections[""+Types.translatedHtml]	= Routes.TreeHtmlTranslated;
			routeRedirections[""+Types.file]			= Routes.TreeFile;
			routeRedirections[""+Types.image]			= Routes.TreeImage;
			RouteRedirections = routeRedirections;

			logHelper.AddLogMessage( $"{nameof(TreeHelper)}: END" );
		}

		internal async Task CreateNode(Cwd cwd, string meta, string data)
		{
			Utils.Assert( cwd != null, nameof(CreateNode), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var pathDb = cwd.PwdDb();
			var pathParentDb = cwd.PwdParentDb();
			meta = meta ?? "";
			data = data ?? "";

			logHelper.AddLogMessage( $"{nameof(CreateNode)}: Check '{pathDb}' and '{pathParentDb}'" );
			{
				var c = await dc.TreeNodes.Where( v=> (v.Path == pathDb) || (v.Path == pathParentDb) ).CountAsync();
				if( c == 0 )
					throw new Utils.TTTException( $"{nameof(CreateNode)}: Parent path '{cwd.PwdParent()}' does not exist" );
				else if( c == 1 )
					{/* Ok */}
				else if( c == 2 )
					throw new Utils.TTTException( $"{nameof(CreateNode)}: Path '{cwd.Pwd()}' already exists" );
				else
					throw new Utils.TTTException( $"{nameof(CreateNode)}: INTERNAL ERROR: Function returned {c} rows ; Expected only 1" );
			}

			logHelper.AddLogMessage( $"{nameof(CreateNode)}: Create node ; meta length: {meta.Length} ; data length: {data.Length}" );
			var node = new Models.TreeNode{ Path		= pathDb,
											ParentPath	= pathParentDb,
											Meta		= meta,
											Data		= data };
			dc.TreeNodes.Add( node );
			await dc.SaveChangesAsync();
		}

		internal async Task<Utils.CustomClass2<MetaData,Models.TreeNode>> GetNode(Cwd cwd)
		{
			Utils.Assert( cwd != null, nameof(GetNode), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var pathDb = cwd.PwdDb();

			logHelper.AddLogMessage( $"{nameof(GetNode)}: Check '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).SingleOrDefaultAsync();
			if( node == null )
				return null;

			MetaData meta;
			if(! node.Meta.StartsWith('{') )
				// Not JSON
				meta = null;
			else
				meta = node.Meta.JSONDeserialize();

			return new Utils.CustomClass2<MetaData, Models.TreeNode>{ A=meta, B=node };
		}
		internal async Task<Utils.CustomClass2<MetaData,Models.TreeNode>> GetNode(Cwd cwd, Types expectedType)
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

		internal async Task<Utils.CustomClass2<int,MetaData>> GetNodeMetaData(Cwd cwd, Types? expectedType=null)
		{
			Utils.Assert( cwd != null, nameof(GetNodeMetaData), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var dc = cwd.DataContext;

			logHelper.AddLogMessage( $"{nameof(GetNodeMetaData)}: Retreive '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).Select( v=>new{ v.ID, v.Meta } ).SingleOrDefaultAsync();
			if( node == null )
				return null;
			if(! node.Meta.StartsWith('{') )
				// Metadata not JSON
				return null;

			logHelper.AddLogMessage( $"{nameof(GetNodeMetaData)}: Parse node's metadata" );
			var dict = node.Meta.JSONDeserialize();
			if( expectedType != null )
				if( dict.TreeMetadata_Type() != ""+expectedType )
					throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not of expected type '{expectedType}'" );

			return new Utils.CustomClass2<int,MetaData>{ A=node.ID, B=dict };
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

		/// <summary>Use direct ADO access to the database to avoid performance issues with EntityFramework and large values</summary>
		internal static async Task<string> GetNodeDataFast(Models.ItemTTTContext dc, string path=null, int? nodeID=null)
		{
			Utils.Assert( dc != null, nameof(GetNodeDataFast), $"Missing parameter '{nameof(dc)}'" );
			Utils.Assert( (path == null) != (nodeID == null), nameof(GetNodeDataFast), $"One and only one of '{nameof(path)}' and '{nameof(nodeID)}' parameters must be set" );

			using( var co = await Utils.ConnectionOpenner.New(dc.Database.GetDbConnection()) )
			using( var comm = co.Connection.CreateCommand() )
			{
				if( nodeID != null )
					comm.CommandText = $"select \"{Models.TreeNode.DataColumnName}\" from \"{Models.TreeNode.TableName}\" where \"{Models.TreeNode.IDColumnName}\" = {nodeID.Value}";
				else
					comm.CommandText = $"select \"{Models.TreeNode.DataColumnName}\" from \"{Models.TreeNode.TableName}\" where \"{Models.TreeNode.PathColumnName}\" = '{path}'";

				using( var rdr = await comm.ExecuteReaderAsync() )
				{
					var rc = rdr.Read();
					if( ! rc )
						// Not found
						return null;

					using( var tr = rdr.GetTextReader(0) )
					{
						return await tr.ReadToEndAsync();
					}
				}
			}
		}

		internal async Task SetNodeMetaData(Cwd cwd, MetaData meta, Types? expectedType=null)
		{
			Utils.Assert( cwd != null, nameof(SetNodeMetaData), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var path = cwd.Pwd();
			var pathDb = cwd.PwdDb();
			var dc = cwd.DataContext;

			logHelper.AddLogMessage( $"{nameof(SetNodeMetaData)}: Retreive '{pathDb}'" );
			var node = await dc.TreeNodes.Where( v=>v.Path == pathDb ).SingleOrDefaultAsync();
			if( node == null )
				throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not found" );

			if( expectedType != null )
			{
				var dict = node.Meta.JSONDeserialize();
				if( dict.TreeMetadata_Type() != ""+expectedType )
					throw new Utils.TTTException( $"{nameof(TreeHelper)}: Node at '{path}' is not of expected type '{expectedType}'" );
			}

			logHelper.AddLogMessage( $"{nameof(SetNodeData)}: Update node's meta data" );
			node.Meta = ( meta?.JSONStringify() ) ?? "";
			await dc.SaveChangesAsync();
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

		internal async Task<int> DelTree(Cwd cwd, bool included=true)
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

			return ids.Count;
		}

		internal async IAsyncEnumerable<string> SaveTree(Cwd cwd, bool excludeImages=false)
		{
			Utils.Assert( cwd != null, nameof(SaveTree), $"Missing parameter '{nameof(cwd)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			var path = cwd.PwdDb();
			logHelper.AddLogMessage( $"{nameof(SaveTree)}: START: '{path}'" );

			var pathBase = path + Cwd.Separator;
			var q = dc.TreeNodes.Where( v=> (v.Path == path) || v.Path.StartsWith(pathBase) )
								.OrderBy( v=>v.Path )
								.Select( v=>new{ v.ID, v.Path, v.Meta } );
			Func<string,object> tryDeserialize = (data)=>
				{
					if( (data.Length > 0)
					 && (	data[0] == '{'
						||	data[0] == '[' ) )
					{
						// Try deserialize JSON
						try { return data.JSONDeserialize<object>(); }
						catch( System.Exception ) {}
					}
					return data;
				};

			// nb: With the 'foreach()', there will be an open DataReader on the main DataContext during the whole streaming
			//		=> Using a separate one to retreive the 'Data' fields
			using( var dataDC = Models.ItemTTTContext.New(ConnectionString) )
			await foreach( var node in q.AsAsyncEnumerable() )
			{
				var meta = tryDeserialize( node.Meta );
				var metaDict = meta as Newtonsoft.Json.Linq.JObject;

				var nodePath = node.Path.Substring( path.Length );
				if( nodePath.Length == 0 )
					// Base path => root
					nodePath = Cwd.Separator;
				logHelper.AddLogMessage( $"{nameof(SaveTree)}: At '{nodePath}'" );

				if( excludeImages && (metaDict != null) )
				{
					if( metaDict.TreeMetadata_Type() == ""+Types.image )
					{
						logHelper.AddLogMessage( $"{nameof(SaveTree)}: Type {Types.image} => Discard" );
						// Discard
						continue;
					}
				}

				// Retreive the data using a separate SQL connection
				var data = await GetNodeDataFast( dataDC, nodeID:node.ID );

				// Output the line
				var line = (new TreeNodeItem {
									Path = nodePath,
									Meta = meta,
									Data = tryDeserialize( data ),
								}).JSONStringify( indented:false );  // nb: Must serialize on 1 line => Ensure 'indented:false'
				yield return line;
			}
		}
		internal async Task RestoreTree(Cwd cwd, string filePath, bool createTransaction=true, bool overwrite=false)
		{
			Utils.Assert( cwd != null, nameof(RestoreTree), $"Missing parameter '{nameof(cwd)}'" );
			if( string.IsNullOrWhiteSpace(filePath) )
				throw new Utils.TTTException( $"{nameof(RestoreTree)}: Missing parameter '{nameof(filePath)}'" );
			var logHelper = cwd.LogHelper;

			logHelper.AddLogMessage( $"{nameof(RestoreTree)}: Validate path '{filePath}'" );
			{
				var segments = filePath.Split( new char[]{'\\','/'}, StringSplitOptions.RemoveEmptyEntries );
				if( segments.Contains("..") )
					throw new Utils.TTTException( $"{nameof(RestoreTree)}: Invalid path '{nameof(filePath)}'" );
				filePath = System.IO.Path.Join( System.IO.Directory.GetCurrentDirectory(), Startup.ViewsLocation , string.Join(System.IO.Path.DirectorySeparatorChar, segments) );
			}

			logHelper.AddLogMessage( $"{nameof(RestoreTree)}: Open file '{filePath}'" );
			using( var file = new System.IO.StreamReader(filePath) )
			{
				await RestoreTree( cwd, file.GetLineEnumerable(), createTransaction:createTransaction, overwrite:overwrite );
			}
		}
		internal async Task RestoreTree(Cwd cwd, IAsyncEnumerable<string> lines, bool createTransaction=true, bool overwrite=false)
		{
			Utils.Assert( cwd != null, nameof(RestoreTree), $"Missing parameter '{nameof(cwd)}'" );
			Utils.Assert( lines != null, nameof(RestoreTree), $"Missing parameter '{nameof(lines)}'" );
			var logHelper = cwd.LogHelper;
			var dc = cwd.DataContext;
			logHelper.AddLogMessage( $"{nameof(RestoreTree)}: START: '{cwd.Pwd()}' ; createTransaction:'{createTransaction}'" );

			using( var transaction = (createTransaction ? await cwd.BeginTransaction() : null) )
			{
				await foreach( var json in lines )
				{
					if( json.Length == 0 )
						continue;
					var node = json.JSONDeserialize<TreeNodeItem>();
					var path = node.Path.TrimStart( Cwd.SeparatorC );  // nb: use relative paths ...
					using( (path.Length > 0) ? cwd.PushDisposable(path) : null )
					{
						logHelper.AddLogMessage( $"{nameof(RestoreTree)}: At path '{cwd.Pwd()}'" );

						if( overwrite )
						{
							var dbPath = cwd.PwdDb();
							var id = await cwd.DataContext.TreeNodes.Where( v=>v.Path == dbPath ).Select( v=>(int?)v.ID ).SingleOrDefaultAsync();
							if( id != null )
							{
								// Node already exists
								await DelTree( cwd, included:true );
							}
						}

						await cwd.TreeHelper.CreateNode( cwd, node.Meta.ToString(), node.Data.ToString() );
						await cwd.DataContext.SaveChangesAsync();
					}
				}

				if( transaction != null )
					await cwd.CommitTransaction( transaction );
			}
		}
		private class TreeNodeItem
		{
			public string Path { get; set; }
			public object Meta { get; set; }
			public object Data { get; set; }
		}

		internal string TryGetRouteRedirection(MetaData metaData)
		{
			var nodeType = metaData.TreeMetadata_Type();
			if( nodeType == null )
				return null;
			return RouteRedirections.TryGet( nodeType );
		}
	}

	internal static partial class ExtensionMethods
	{
		internal static string TreeMetadata_Type(this Newtonsoft.Json.Linq.JObject metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_Type), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "type" );
		}
		internal static string TreeMetadata_Type(this MetaData metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_Type), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "type" );
		}
		internal static void TreeMetadata_Type(this MetaData metadata, string value)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_Type), $"Missing parameter '{nameof(metadata)}'" );
			if( value == null )
				metadata.Remove( "type" );
			else
				metadata[ "type" ] = value;
		}
	}
}
