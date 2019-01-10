
using System;
using System.Collections.Generic;
using System.Linq;

using ItemTTT;

namespace ItemTTT.DTOs
{
	public class BlogPost
	{
		public int?		ID				{ get; set; }
		public string	Date			{ get; set; }
		public string	ImageHtml		{ get; set; }
		public string	TitleHtmlEN		{ get; set; }
		public string	TitleHtmlFR		{ get; set; }
		public string	TitleHtmlNL		{ get; set; }
		public string	TextHtmlEN		{ get; set; }
		public string	TextHtmlFR		{ get; set; }
		public string	TextHtmlNL		{ get; set; }
		public bool		Active			{ get; set; }

		public string	UrlPrevious		{ get; set; }
		public string	UrlNext			{ get; set; }

		public BlogPost(Models.BlogPost src=null)
		{
			if( src == null )
				return;

			ID			= src.ID;
			Date		= src.Date.ToIsoDate();
			ImageHtml	= src.ImageHtml;
			TitleHtmlEN	= src.TitleHtmlEN;
			TitleHtmlFR	= src.TitleHtmlFR;
			TitleHtmlNL	= src.TitleHtmlNL;
			TextHtmlEN	= src.TextHtmlEN;
			TextHtmlFR	= src.TextHtmlFR;
			TextHtmlNL	= src.TextHtmlNL;
			Active		= src.Active;
		}
	}
}
