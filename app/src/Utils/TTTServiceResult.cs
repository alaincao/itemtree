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
				if( (! Success) || Utils.IsDebug )
					Log = pageHelper.ScopeLogs.GetLogLines();
			}

			internal static TTTServiceResult LogAndNew(PageHelper pageHelper, System.Exception ex)
			{
				var errorMessage = LogAndReturnErrorMessage( pageHelper, ex );
				return new TTTServiceResult( pageHelper, errorMessage:errorMessage );
			}
			protected static string LogAndReturnErrorMessage(PageHelper pageHelper, System.Exception ex)
			{
				pageHelper.ScopeLogs.AddException( ex );

				if( ex is Utils.TTTException )
					return ex.Message;

				// Search exception tree's messages for known error triggers
				for( var current=ex; current != null; current = current.InnerException )
				{
					foreach( var pair in Models.ItemTTTContext.KnownErrorTriggers )
					{
						if( current.Message.Contains(pair.Key) )
							// Found! return that error message
							return pair.Value;
					}
				}
				// Not found

				return $"An unexpected error occurred ({ex.GetType().FullName}): {ex.Message}";
			}
		}

		public class TTTServiceResult<T> : TTTServiceResult
		{
			public T		Result			{ get; set; }

			public TTTServiceResult(PageHelper pageHelper, string errorMessage=null) : base(pageHelper, errorMessage)  {}

			internal static TTTServiceResult<T> LogAndNew(PageHelper pageHelper, System.Exception ex, T result=default(T))
			{
				var errorMessage = LogAndReturnErrorMessage( pageHelper, ex );
				return new Utils.TTTServiceResult<T>( pageHelper, errorMessage:errorMessage );
			}
		}
	}
}
