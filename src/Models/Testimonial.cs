
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemTTT.Models
{
	[Table("Testimonials")]
	public class Testimonial
	{
		[Key]
		[Column("TestimonialID")]
		public int		ID				{ get; set; }

		[Column("TestimonialDate", TypeName = "date")]
		public DateTime	Date			{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	FirstLastName	{ get; set; }

		[Required]
		[MaxLength(255)]
		public string	WhosWho			{ get; set; }

		[Required]
		[Column("Testimonial", TypeName = "nvarchar(max)")]
		public string	Text			{ get; set; }

		public bool		Active			{ get; set; }

		[Column("ImageTestimonial", TypeName = "nvarchar(max)")]
		public string	ImageData		{ get; set; }
	}
}
