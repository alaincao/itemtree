using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemTTT
{
	internal class LogHelper
	{
		private struct Entry
		{
			internal DateTime	Date;
			internal string		Message;
		}

		private List<Entry>		Messages	= new List<Entry>();
		public bool				HasErrors	= false;

		internal void AddLogMessage(string message)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(message), this, $"Missing parameter {nameof(message)}" );

			Messages.Add( new Entry{ Date=DateTime.Now, Message=message } );
			Utils.Log( this, message );
		}

		internal void AddLogLines(string messageText)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(messageText), GetType(), $"Missing parameter {nameof(messageText)}" );
			if( messageText == null )
				return;
			AddLogLines( messageText.Split('\n') );
		}

		internal void AddLogLines(IEnumerable<string> lines)
		{
			Utils.Assert( lines != null, GetType(), $"Missing parameter {nameof(lines)}" );
			if( lines == null )
				return;
			foreach( var line in lines )
				AddLogMessage( line );
		}

		internal void AddErrorMessage(string message)
		{
			if( Utils.IsDebug )
				// Add the log to the page's JavaScript console
				HasErrors = true;
			AddLogMessage( message );  // Same thing for now... Maybe implement later ...
		}
		internal void AddWarningMessage(string message)
		{
			AddLogMessage( message );  // Same thing for now... Maybe implement later ...
		}

		internal void AddException(Exception exception, bool asLogMessages=false)
		{
			try
			{
				Utils.Assert( exception != null, GetType(), $"Missing parameter {nameof(exception)}" );
				if( asLogMessages )
					{/* Not a real error */}
				else
					HasErrors = true;

				var aggregateException = exception as AggregateException;
				if( aggregateException != null )
				{
					// async/await strange exceptions ...
					aggregateException = aggregateException.Flatten();
					Utils.Assert( aggregateException.InnerExceptions.Count() == 1, GetType(), "There are more than 1 exception in the 'AggregateException'; Only one will be logged ..." );  // What to do with the others ??

					// Use the 'InnerExceptions' which is the one we are really interrested in
					AddException( aggregateException.InnerException, asLogMessages );

					// And from the original exception, only output its StackTrace
					AddLogMessage( "--- Stack trace from AggregateException:" );
					AddLogLines( exception.StackTrace );
					return;
				}

				AddLogMessage( $"*** Exception ({exception.GetType().FullName}): {exception.Message}" );
				AddLogLines( exception.StackTrace );
			}
			catch( System.Exception ex )
			{
				Utils.Fail( GetType(), $"'LogHelper.AddException()' threw an exception ({ex.GetType().FullName}): {ex.Message}" );
			}

			if( exception.InnerException != null )
			{
				AddException( exception:exception.InnerException, asLogMessages:true );
			}
		}

		internal void Merge(LogHelper src)
		{
			Messages = Messages
						.Concat( src.Messages )
							.OrderBy( v=>v.Date )
							.ToList();
		}

		internal string[] GetLogLines()
		{
			var format = @"hh\:mm\:ss\.fff";
			return Messages.Select( (entry)=>$"{entry.Date.ToString(format)}: {entry.Message}" ).ToArray();
		}

		internal string GetLogs()
		{
			return string.Join( "\n", GetLogLines() );
		}
	}
}
