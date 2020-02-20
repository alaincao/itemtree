using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("Item")]
	public class Item
	{
		[Key]
		[Column("ItemID")]
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

		public int?		Price			{ get; set; }

		[Column(TypeName = "varchar(max)")]
		public string	Features		{ get; set; }

		public bool		Active			{ get; set; }

		public int?		MainImageNumber	{ get; set; }

		public List<ItemPicture>	Pictures		{ get; set; }
	}
}
