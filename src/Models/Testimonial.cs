
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

		internal static IQueryable<Testimonial> UnselectImage(IQueryable<Testimonial> query)
		{
			return query.Select( v=>new Testimonial{
											ID				= v.ID,
											Date			= v.Date,
											FirstLastName	= v.FirstLastName,
											WhosWho			= v.WhosWho,
											Text			= v.Text,
											Active			= v.Active,
											ImageData		= null,
										} );
		}
	}
}
