using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemTTT.Common
{
	/// <summary>Class returned by API URLs that only needs to return generic information (i.e. too lazy to create a class for it ...)</summary>
	public class TTTServiceResult
	{
		public bool?	Success			{ get; set; }
		public string	ErrorMessage	{ get; set; }
		public string[]	Log				{ get; set; }
	}
}
