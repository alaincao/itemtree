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

		internal void AddLogMessage(string message)
		{
			Utils.Assert( !string.IsNullOrWhiteSpace(message), this, "Missing parameter 'message'" );

			Messages.Add( new Entry{ Date=DateTime.Now, Message=message } );
			Utils.Log( this, message );
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
