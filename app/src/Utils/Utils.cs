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
				// TODO
				Utils.Log( sender, $"*** ASSERTION FAILED: '{message}'" );
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
	}
}
