using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemTTT
{
	public static partial class Utils
	{
		/// <summary>Class returned by API URLs that only needs to return generic information (i.e. too lazy to create a class for it ...)</summary>
		public class TTTServiceResult
		{
			public bool		Success			{ get { return (ErrorMessage == null); } }
			public string	ErrorMessage	{ get; set; }
			public string[]	Log				{ get; set; }

			public TTTServiceResult(PageHelper pageHelper, string errorMessage=null)
			{
				ErrorMessage = errorMessage;
				if( Utils.IsDebug )
					Log = pageHelper.ScopeLogs.GetLogLines();
			}

			public static TTTServiceResult LogAndNew(PageHelper pageHelper, System.Exception ex)
			{
				pageHelper.ScopeLogs.AddException( ex );
				return new TTTServiceResult( pageHelper, errorMessage:$"An unexpected error occurred ({ex.GetType().FullName}): {ex.Message}" );
			}
		}

		public class TTTServiceResult<T>
		{
			public bool		Success			{ get { return (ErrorMessage == null); } }
			public T		Result			{ get; set; }
			public string	ErrorMessage	{ get; set; }
			public string[]	Log				{ get; set; }

			public TTTServiceResult(PageHelper pageHelper, string errorMessage=null)
			{
				ErrorMessage = errorMessage;
				if( Utils.IsDebug )
					Log = pageHelper.ScopeLogs.GetLogLines();
			}

			public static TTTServiceResult<T> LogAndNew(PageHelper pageHelper, System.Exception ex, T result=default(T))
			{
				pageHelper.ScopeLogs.AddException( ex );
				return new TTTServiceResult<T>( pageHelper, errorMessage:$"An unexpected error occurred ({ex.GetType().FullName}): {ex.Message}" );
			}
		}
	}
}
