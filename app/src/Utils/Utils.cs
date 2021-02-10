using System;

namespace ItemTTT
{
	public static partial class Utils
	{
		public static bool		IsDebug;

		static Utils()
		{
			IsDebug = false;
		}

		// TODO: ACA: Assertion & conditional compilation
		public static void Assert(bool test, object sender, string message)
		{
			if(! test )
			{
				Utils.Log( sender, $"*** ASSERTION FAILED: '{message}'" );
				if( IsDebug )
					System.Diagnostics.Debug.Fail( message );
			}
		}

		// TODO: ACA: Assertion & conditional compilation
		public static void Fail(object sender, string message)
		{
			Assert( false, sender, message );
		}

		// TODO: ACA: Assertion & conditional compilation
		public static void Log(object sender, string message)
		{
			if( IsDebug )
				System.Console.WriteLine( message );
		}

		internal static IDisposable NewDisposable(Action dispose)
		{
			return new Disposable( dispose );
		}
		private class Disposable : IDisposable
		{
			private readonly Action	DisposeCallback;
			internal Disposable(Action dispose)	{ DisposeCallback = dispose; }
			void IDisposable.Dispose()			{ DisposeCallback?.Invoke(); }
		}

		internal static string GetMd5Sum(string str)
		{
			using( var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider() )
			{
				var bytes = System.Text.Encoding.UTF8.GetBytes( str );
				bytes = md5.ComputeHash( bytes );

				var sb = new System.Text.StringBuilder();
				for( var i=0; i < bytes.Length; ++i )
					sb.Append( bytes[i].ToString("x2") );

				return sb.ToString();
			}
		}

		internal static string GetRandomString(int length, string charset="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
		{
			var data = new char[length];
			var charSetLength = charset.Length;
			for( var i=0; i<length; ++i )
			{
				var idx = System.Security.Cryptography.RandomNumberGenerator.GetInt32( charSetLength );
				var c = charset[ idx ];
				data[ i ] = c;
			}
			return new string( data );
		}
	}
}
