
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("BlogPost")]
	public class BlogPost
	{
		[Key]
		[Column("BlogPostID")]
		public int		ID				{ get; set; }

		[Column(TypeName = "date")]
		public DateTime	PostDate		{ get; set; }

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
	}
}
