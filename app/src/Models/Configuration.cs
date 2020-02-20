using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("Configuration")]
	public class Configuration
	{
		internal const string	Key_PasswordHash			= "AdminPasswordHash";

		[Key]
		[Column("ConfigurationID")]
		public int		ID		{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	Key		{ get; set; }

		[Column(TypeName = "varchar(max)")]
		public string	Value	{ get; set; }
	}
}
