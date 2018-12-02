
namespace ItemTTT.DTOs
{
	public class ItemPicture
	{
		internal static readonly ItemPicture	Empty		= new ItemPicture();

		public string			ItemCode			{ get; set; } = null;
		public int				Number				{ get; set; } = 0;
		public bool				IsMainImage			{ get; set; } = false;
		public string			UrlOriginal			{ get; set; } = wwwroot.ImgNotFound;
		public string			Url100				{ get; set; } = wwwroot.ImgNotFound;
		public string			Url133				{ get; set; } = wwwroot.ImgNotFound;
		public string			Url260				{ get; set; } = wwwroot.ImgNotFound;
	}
}
