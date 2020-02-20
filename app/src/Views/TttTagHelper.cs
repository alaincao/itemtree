using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ItemTTT.Views
{

// Manages all tags with 'ttt-XXX' attributes. E.g. "<div ttt-only-if="IsEN">Hello</div>"
// NB: If this is not working, check file "_ViewImports.cshtml": the line "@addTagHelper *, <DLL>" line must reference the current DLL name

	[HtmlTargetElement(Attributes = "ttt-only-if")]
	public class TttTagHelper : TagHelper
	{
		[HtmlAttributeName("ttt-only-if")]
		public bool?		OnlyIf		{ get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if( OnlyIf != null )
			{
				if( OnlyIf.Value == false )
				{
					output.SuppressOutput();
					return;
				}
			}

			output.Attributes.RemoveAll( "ttt-only-if" );
		}
	}
}
