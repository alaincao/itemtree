using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("TreeNode")]
	public class TreeNode
	{
		[Key, Column("TreeNodeID")]
		public int		ID				{ get; set; }

		[Required, MaxLength(4000)]
		public string	Path			{ get; set; }

		[MaxLength(4000)]
		public string	ParentPath		{ get; set; }

		[Required, MaxLength(4000)]
		public string	Meta			{ get; set; }

		[Required, Column(TypeName = "nvarchar(max)")]
		public string	Data			{ get; set; }
	}
}
