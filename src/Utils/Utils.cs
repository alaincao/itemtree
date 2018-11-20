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
	}
}
