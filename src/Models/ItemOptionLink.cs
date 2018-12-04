using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("CarOptionLink")]
	public class ItemOptionLink
	{
		[Key]
		[Column("CarOptionLinkID")]
		public int		ID				{ get; set; }

		[Column("CarID")]
		public int		ItemID			{ get; set; }

		[Required]
		[Column("CarOptionID")]
		public int		ItemOptionID	{ get; set; }

		public Item			Item	{ get; set; }
		public ItemOption	Option	{ get; set; }
	}
}
