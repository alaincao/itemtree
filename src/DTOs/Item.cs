
using System.Collections.Generic;
using System.Linq;

namespace ItemTTT.DTOs
{
	public class Item
	{
		public string	Code			{ get; set; }
		public string	Name			{ get; set; }
		public string	DescriptionEN	{ get; set; }
		public string	DescriptionFR	{ get; set; }
		public string	DescriptionNL	{ get; set; }
		public int?		Price			{ get; set; }
		public bool		Active			{ get; set; }

		public Dictionary<Languages,string>	DetailsUrls	{ get; set; }
		public ItemPicture[]				Pictures	{ get; set; }
		public Utils.Translation[]			Options		{ get; set; }

		internal int 	MainImageNumber	= 1;  // nb: Not transmitted to client
		public ItemPicture		FirstImage	{ get { return Pictures == null ? ItemPicture.Empty : Pictures.FirstOrDefault(); } }
		public ItemPicture		MainImage	{ get { return Pictures == null ? ItemPicture.Empty : Pictures.Where(v=>v.Number == MainImageNumber).SingleOrDefault(); } }

		public Item(Models.Item src)
		{
			Code			= src.Code;
			Name			= src.Name;
			DescriptionEN	= src.DescriptionEN;
			DescriptionFR	= src.DescriptionFR;
			DescriptionNL	= src.DescriptionNL;
			MainImageNumber	= src.MainImageNumber ?? 1;
			Price			= src.Price;
			Active			= src.Active;
		}
	}
}
