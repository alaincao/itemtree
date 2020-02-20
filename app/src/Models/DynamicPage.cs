using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("DynamicPage")]
	public class DynamicPage
	{
		[Key]
		[Column("DynamicPageID")]
		public int		ID				{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	Code			{ get; set; }

		[Required]
		[Column(TypeName = "nvarchar(max)")]
		public string	TranslationEN	{ get; set; }

		[Required]
		[Column(TypeName = "nvarchar(max)")]
		public string	TranslationFR	{ get; set; }

		[Required]
		[Column(TypeName = "nvarchar(max)")]
		public string	TranslationNL	{ get; set; }
	}
}
