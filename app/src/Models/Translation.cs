using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	using Types = Services.TranslationController.Types;

	[Table("Translation")]
	public class Translation
	{
		[Key]
		[Column("TranslationID")]
		public int		ID				{ get; set; }

		[Required]
		[MaxLength(255)]
		[Column("Type")]
		public string	TypeString		{ get; set; }
		internal Types	Type			{ get { return TypeString.TryParseEnum<Types>() ?? Types.Undefined; } set { TypeString = ""+value; } }

		[Required]
		[MaxLength(255)]
		public string	TranslationEN	{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	TranslationFR	{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	TranslationNL	{ get; set; }
	}
}
