using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("Car")]
	public class Item
	{
		[Key]
		[Column("CarID")]
		public int		ID				{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	Code			{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	Name			{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	DescriptionEN	{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	DescriptionFR	{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	DescriptionNL	{ get; set; }

		[Required]
		public bool		Active			{ get; set; }
	}
}
