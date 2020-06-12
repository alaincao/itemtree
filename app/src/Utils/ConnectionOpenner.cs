
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace ItemTTT
{
	public static partial class Utils
	{
		public class ConnectionOpenner : IDisposable
		{
			public DbConnection	Connection			{ get; private set; }
			private bool		ConnectionWasOpen;

			/// <remarks>Use in synchroneous methods</remarks>
			public ConnectionOpenner(DbConnection connection)
			{
				Utils.Assert( connection != null, typeof(ConnectionOpenner), $"Missing parameter '{nameof(connection)}'" );

				Connection = connection;
				ConnectionWasOpen = (connection.State == System.Data.ConnectionState.Open);
				if(! ConnectionWasOpen )
					Connection.Open();

				Utils.Assert( connection.State == System.Data.ConnectionState.Open, typeof(ConnectionOpenner), "Connection is not open" );
			}

			private ConnectionOpenner(DbConnection connection, bool connectionWasOpen)
			{
				Connection			= connection;
				ConnectionWasOpen	= connectionWasOpen;
			}

			/// <remarks>Use in asynchroneous methods</remarks>
			public static async Task<ConnectionOpenner> New(DbConnection connection)
			{
				var wasOpen = (connection.State == System.Data.ConnectionState.Open);
				if(! wasOpen )
				{
					await connection.OpenAsync();
				}
				return new ConnectionOpenner( connection, wasOpen );  // nb: 'Close()' must be synchroneous because of 'Dispose()' ; Plus, would be too dangerous to be running in a separate task (i.e. Task.Run(()=>Close()) ) ...
			}

			public void Dispose()
			{
				if(! ConnectionWasOpen )
					try { Connection.Close(); }
					catch( System.Exception ex )  { Utils.Assert( false, typeof(ConnectionOpenner), $"Could not close connection: {ex.Message}" ); };
			}
		}
	}
}
