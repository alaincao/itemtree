
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ItemTTT.Tree
{
	public class Cwd
	{
		internal const string	Separator	= "/";
		internal const char		SeparatorC	= '/';
		internal const string	RefCurrent	= ".";

		internal readonly LogHelper				LogHelper;
		internal readonly Models.ItemTTTContext	DataContext;
		internal readonly TreeHelper			TreeHelper;
		private readonly Stack<string[]>		Dirs;

		public Cwd(IServiceProvider services) : this(	logHelper	: services.GetScopeLog(),
														dataContext	: services.GetRequiredService<Models.ItemTTTContext>(),
														treeHelper	: services.GetRequiredService<TreeHelper>() )
		{
		}

		private Cwd(LogHelper logHelper, Models.ItemTTTContext dataContext, TreeHelper treeHelper)
		{
			LogHelper = logHelper;
			DataContext = dataContext;
			TreeHelper = treeHelper;
			Dirs = new Stack<string[]>();
			Dirs.Push( new[]{ TreeHelper.Root } );
		}

		internal static Cwd New(LogHelper logHelper, IServiceProvider services, string rootSuffix=null)
		{
			var startup = services.GetRequiredService<Startup>();
			var dataContext = services.GetRequiredService<Models.ItemTTTContext>();
			var root = startup.Configuration[ AppSettingsKeys.TreeRoot ] + rootSuffix;
			var treeHelper = new Tree.TreeHelper( logHelper, startup.ConnectionString, root );
			var cwd = new Tree.Cwd( logHelper, dataContext, treeHelper );
			return cwd;
		}

		/// <summary>Ensure root folder node exists</summary>
		internal async Task EnsureRootFolder(string rootSuffix=null)
		{
			var rootPathDb = PwdDb( "/" );
			var rootNode = await DataContext.TreeNodes.Where( v=>v.Path == rootPathDb ).SingleOrDefaultAsync();
			if( rootNode != null )
				// Already exists
				return;
			// Does not exist => Create it
			rootNode = new Models.TreeNode{ Path=rootPathDb, Meta="", Data="" };
			DataContext.Add( rootNode );
			await DataContext.SaveChangesAsync();
		}

		internal Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransaction()
		{
			return DataContext.Database.BeginTransactionAsync();
		}
		internal async Task CommitTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
		{
			await transaction.CommitAsync();
		}

		public void Cd(string path)
		{
			var segments = GetSegments( path );
			Dirs.Pop();
			Dirs.Push( segments );
		}

		internal string GetNodeName(string path=null)
		{
			string[] segments;
			if( path == null )
				segments = Dirs.Peek();
			else
				segments = GetSegments( path );

			if( segments.Length == 0 )
				// Root?
				return null;
			return segments[ segments.Length-1 ];
		}

		internal string Pwd(string dbPath=null)
		{
			if( dbPath == null )
				return GetPath( Dirs.Peek() );

			return GetPath( dbPath.Split(SeparatorC, StringSplitOptions.RemoveEmptyEntries) );
		}
		internal string PwdDb()
		{
			return GetPathDb( Dirs.Peek() );
		}
		internal string PwdDb(string path)
		{
			path = SanitizePath( path );
			var segments = GetSegments( path );
			return GetPathDb( segments );
		}

		internal string PwdParent()
		{
			var parent = GetParentSegments();
			return GetPath( parent );
		}
		internal string PwdParentDb()
		{
			var parent = GetParentSegments();
			return GetPathDb( parent );
		}
		private string[] GetParentSegments()
		{
			var dir = Dirs.Peek();
			if( dir.Length == 1 )
				throw new Utils.TTTException( $"{nameof(Cwd)}.{nameof(PwdParentDb)}: Trying to get root node's parent" );
			var parent = dir.Take( dir.Length-1 ).ToArray();
			return parent;
		}

		public void Pushd(string path)
		{
			var segments = GetSegments( path );
			Dirs.Push( segments );
		}

		public string Popd()
		{
			return GetPath( Dirs.Pop() );
		}

		public IDisposable PushDisposable(string path)
		{
			Pushd( path );
			return Utils.NewDisposable( ()=>Popd() );
		}

		private static string GetPath(string[] segments)
		{
			return Separator + string.Join( Separator, segments.Skip(1) );  // eg. "/xx/yy" (without the "root" prefix)
		}
		private static string GetPathDb(string[] segments)
		{
			return string.Join( Separator, segments );  // eg. "root/xx/yy" (with the "root" prefix)
		}

		internal static string SanitizePath(string path)
		{
			var chars = path.Select( c=>
								{
									if( (c >= 'a') && (c <= 'z') )
										return c;  // Ok
									if( (c >= '0') && (c <= '9') )
										return c;  // Ok
									if( (c >= 'A') && (c <= 'Z') )
										return (char)( c-('A'-'a') );  // Lower case
									if( (c == SeparatorC) || (c == '-') || (c == '.') )
										return c;  // Ok
									if( c == ' ' )
										return '-';  // Replace spaces by '-'
									return '_';  // Others => Replace by '_'
								} )
							.ToArray();
			return new string( chars );
		}
		internal static string SanitizeName(string name)
		{
			// Same as 'SanitizePath()', but replace separator also ...
			return SanitizePath( name ).Replace( SeparatorC, '_' );
		}

		private string[] GetSegments(string path)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(path), typeof(Cwd), $"Missing parameter {nameof(path)}" );
			path = SanitizePath( path );

			IEnumerable<string> segments;
			if( path.StartsWith(SeparatorC) )
			{
				// Absolute path
				segments = path.Split( SeparatorC, StringSplitOptions.RemoveEmptyEntries );
				segments = (new[]{ TreeHelper.Root }).Concat( segments );  // Prepend 'Root'
			}
			else // Relative path
			{
				segments = Dirs.Peek();  // Starts with "cwd" ...
				segments = segments.Concat( path.Split(SeparatorC, StringSplitOptions.RemoveEmptyEntries) );  // ... then append from specified path
			}
			segments = segments.Where( v=>v != RefCurrent );  // Remove all '.'

			return segments.ToArray();
		}
	}
}
