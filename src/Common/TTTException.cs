using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemTTT.Common
{
	public class TTTException : System.ApplicationException
	{
		public TTTException(string message) : base(message)  {}
	}
}
