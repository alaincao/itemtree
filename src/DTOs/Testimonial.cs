
using System;
using System.Collections.Generic;
using System.Linq;

using ItemTTT;

namespace ItemTTT.DTOs
{
	public class Testimonial
	{
		public int?		ID				{ get; set; }
		public string	Date			{ get; set; }
		public string	FirstLastName	{ get; set; }
		public string	WhosWho			{ get; set; }
		public string	Text			{ get; set; }
		public bool		Active			{ get; set; }
		public string	ImageData		{ get; set; }

		public Testimonial(Models.Testimonial src=null)
		{
			if( src == null )
			{
				Date = DateTime.Now.ToIsoDate();
				return;
			}

			ID				= src.ID;
			Date			= src.Date.ToIsoDate();
			FirstLastName	= src.FirstLastName;
			WhosWho			= src.WhosWho;
			Text			= src.Text;
			Active			= src.Active;
			ImageData		= src.ImageData;
		}

		internal void ToModel(Models.Testimonial dst)
		{
			var date = (Date != null) ? (Date.FromIsoDate()??DateTime.Now) : DateTime.Now;

			dst.Date			= date;
			dst.FirstLastName	= FirstLastName	?? "";
			dst.WhosWho			= WhosWho		?? "";
			dst.Text			= Text			?? "";
			dst.Active			= Active;
			dst.ImageData		= ImageData		?? "";
		}
	}
}
