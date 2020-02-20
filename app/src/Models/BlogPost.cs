
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ItemTTT.Models
{
	[Table("BlogPost")]
	public class BlogPost
	{
		[Key]
		[Column("BlogPostID")]
		public int		ID				{ get; set; }

		[Column(TypeName = "date")]
		public DateTime	Date			{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	ImageHtml		{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	TitleHtmlEN		{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	TitleHtmlFR		{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	TitleHtmlNL		{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	TextHtmlEN		{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	TextHtmlFR		{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	TextHtmlNL		{ get; set; }

		public bool		Active			{ get; set; }

		internal static IQueryable<BlogPost> UnselectImage(IQueryable<BlogPost> query)
		{
			return query.Select( v=>new BlogPost{
											ID			= v.ID,
											Date		= v.Date,
											ImageHtml	= null,
											TitleHtmlEN	= v.TitleHtmlEN,
											TitleHtmlFR	= v.TitleHtmlFR,
											TitleHtmlNL	= v.TitleHtmlNL,
											TextHtmlEN	= v.TextHtmlEN,
											TextHtmlFR	= v.TextHtmlFR,
											TextHtmlNL	= v.TextHtmlNL,
											Active		= v.Active,
										} );
		}
	}
}
