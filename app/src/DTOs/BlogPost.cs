
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

		public string	Url				{ get; set; }
		public string	UrlEdit			{ get; set; }
		public string	UrlPrevious		{ get; set; }
		public string	UrlNext			{ get; set; }

		public BlogPost(Models.BlogPost src=null)
		{
			if( src == null )
			{
				Date = DateTime.Now.ToIsoDate();
				return;
			}

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

		internal void ToModel(Models.BlogPost dst)
		{
			var date = (Date != null) ? (Date.FromIsoDate()??DateTime.Now) : DateTime.Now;

			dst.Date		= date;
			dst.ImageHtml	= ImageHtml		?? "";
			dst.TitleHtmlEN	= TitleHtmlEN	?? "";
			dst.TitleHtmlFR	= TitleHtmlFR	?? "";
			dst.TitleHtmlNL	= TitleHtmlNL	?? "";
			dst.TextHtmlEN	= TextHtmlEN	?? "";
			dst.TextHtmlFR	= TextHtmlFR	?? "";
			dst.TextHtmlNL	= TextHtmlNL	?? "";
			dst.Active		= Active;
		}
	}
}
