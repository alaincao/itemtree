
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public Cwd(IServiceProvider services)
		{
			LogHelper = services.GetScopeLog();
			DataContext = services.GetRequiredService<Models.ItemTTTContext>();
			TreeHelper = services.GetRequiredService<TreeHelper>();
			Dirs = new Stack<string[]>();
			Dirs.Push( new[]{ TreeHelper.Root } );
		}

		internal Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransaction()
		{
			return DataContext.Database.BeginTransactionAsync();
		}
		internal async Task CommitTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
		{
			await transaction.CommitAsync();
		}

		internal void Cd(string path)
		{
			var segments = GetSegments( path );
			Dirs.Pop();
			Dirs.Push( segments );
		}

		internal string Pwd()
		{
			return GetPath( Dirs.Peek() );
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

		internal void Pushd(string path)
		{
			var segments = GetSegments( path );
			Dirs.Push( segments );
		}

		internal string Popd()
		{
			return GetPath( Dirs.Pop() );
		}

		internal IDisposable PushDisposable(string path)
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
									return '_';  // Invalid => Replace by '_'
								} )
							.ToArray();
			return new string( chars );
		}
		internal static string SanitizeName(string name)
		{
			// Same as 'Sanitize()', but replace separator also ...
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
