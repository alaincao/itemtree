
namespace ItemTTT.DTOs
{
	using Types = Services.TranslationController.Types;

	public class Translation
	{
		public string	EN;
		public string	FR;
		public string	NL;

		internal void ToModel(Models.Translation dst)
		{
			dst.TranslationEN = EN;
			dst.TranslationFR = FR;
			dst.TranslationNL = NL;
		}

		internal void Sanitize()
		{
			if( EN != null )
				EN = EN.Trim();

			if( string.IsNullOrWhiteSpace(FR) )
				FR = EN;
			else
				FR = FR.Trim();

			if( string.IsNullOrWhiteSpace(NL) )
				NL = EN;
			else
				NL = NL.Trim();
		}
	}

	public class TranslationItem : Translation
	{
		public bool		InOriginal;
		public bool		InTranslation;

		public TranslationItem()
		{
		}

		internal TranslationItem(string en)
		{
			EN = en;
			FR = en;
			NL = en;
		}

		internal TranslationItem(Models.Translation src)
		{
			EN = src.TranslationEN;
			FR = src.TranslationFR;
			NL = src.TranslationNL;
		}
	}
}
