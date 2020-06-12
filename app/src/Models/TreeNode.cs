using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table(TableName)]
	public class TreeNode
	{
		internal const string	TableName		= "TreeNode";
		internal const string	IDColumnName	= "TreeNodeID";
		internal const string	PathColumnName	= nameof(Path);
		internal const string	DataColumnName	= nameof(Data);

		[Key, Column(IDColumnName)]
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
