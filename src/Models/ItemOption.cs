using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("CarOption")]
	public class ItemOption
	{
		[Key]
		[Column("CarOptionID")]
		public int		ID		{ get; set; }

		public int		Order	{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	NameEN	{ get; set; }

		public List<ItemOptionLink>		Links	{ get; set; }
	}
}
