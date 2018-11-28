using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("CarPicture")]
	public class ItemPicture
	{
		internal const string	Type_Original		= "original";
		internal const string	Type_HeightPrefix	= "height=";

		[Key]
		[Column("CarPictureID")]
		public int		ID			{ get; set; }

		[Required]
		[Column("CarID")]
		public int		ItemID		{ get; set; }

		[Required]
		public int		Number		{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	Type		{ get; set; }

		[Required]
		[Column(TypeName = "varchar(max)")]
		public string	Content		{ get; set; }

		public Item		Item		{ get; set; }
	}
}
