using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ItemTTT.Tree
{
	public abstract class TagHelperBase : TagHelper
	{
		protected const string	CsAttributeHtml				= "tree-html";
		protected const string	CsAttributeHtmlTranslated	= "tree-htmlTranslated";
		protected const string	CsAttributeImage			= "tree-image";
		protected const string	JsAttributePath				= "tree-path";
		protected const string	JsAttributeType				= "tree-type";
		protected const string	AttributeParameters			= "tree-parameters";

		protected readonly PageHelper				PageHelper;
		protected readonly Models.ItemTTTContext	DataContext;
		protected readonly Tree.Cwd					Cwd;

		protected TagHelperBase(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
			Cwd = cwd;
		}
	}

	[HtmlTargetElement(Attributes = TagHelperBase.CsAttributeHtml)]
	public class HtmlTagHelper : TagHelperBase
	{
		[HtmlAttributeName(TagHelperBase.CsAttributeHtml)]
		public string		Path		{ get; set; }

		public HtmlTagHelper(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd) : base(pageHelper, dataContext, cwd)  {}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(HtmlTagHelper)}: Start '{Path}'" );

			using( Cwd.PushDisposable(Path) )
			{
				output.Attributes.RemoveAll( CsAttributeHtml );
				if( PageHelper.IsAuthenticated )
				{
					output.Attributes.Add( JsAttributeType, ""+TreeHelper.Types.html );
					output.Attributes.Add( JsAttributePath, Cwd.Pwd() );
				}

				var content = await Cwd.TreeHelper.GetNodeData( Cwd );
				output.Content.SetHtmlContent( content );
			}
			logHelper.AddLogMessage( $"{nameof(HtmlTagHelper)}: END '{Path}'" );
		}
	}

	[HtmlTargetElement(Attributes = TagHelperBase.CsAttributeHtmlTranslated)]
	public class HtmlTranslatedTagHelper : TagHelperBase
	{
		[HtmlAttributeName(TagHelperBase.CsAttributeHtmlTranslated)]
		public string		Path		{ get; set; }

		public HtmlTranslatedTagHelper(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd) : base(pageHelper, dataContext, cwd)  {}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(HtmlTranslatedTagHelper)}: Start '{Path}'" );

			using( Cwd.PushDisposable(Path) )
			{
				output.Attributes.RemoveAll( CsAttributeHtmlTranslated );
				if( PageHelper.IsAuthenticated )
				{
					output.Attributes.Add( JsAttributeType, ""+TreeHelper.Types.translatedHtml );
					output.Attributes.Add( JsAttributePath, Cwd.Pwd() );
				}

				var json = await Cwd.TreeHelper.GetNodeData( Cwd );
				var content = string.IsNullOrWhiteSpace(json) ? "" : TreeController.GetTranslatedNodeText( PageHelper, json );

				output.Content.SetHtmlContent( content );
			}
			logHelper.AddLogMessage( $"{nameof(HtmlTranslatedTagHelper)}: END '{Path}'" );
		}
	}

	[HtmlTargetElement(Attributes = TagHelperBase.CsAttributeImage)]
	public class ImageTagHelper : TagHelperBase
	{
		[HtmlAttributeName(TagHelperBase.CsAttributeImage)]
		public string		Path		{ get; set; }
		[HtmlAttributeName(TagHelperBase.AttributeParameters)]
		public string		Parms		{ get; set; }

		public ImageTagHelper(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd) : base(pageHelper, dataContext, cwd)  {}

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(ImageTagHelper)}: Start '{Path}'" );

			using( Cwd.PushDisposable(Path) )
			{
				var src = Cwd.Pwd();
				if( !string.IsNullOrWhiteSpace(Parms) )
					src += "?"+Parms;

				output.Attributes.Add( "src", src );

				if( PageHelper.IsAuthenticated )
				{
					output.Attributes.Add( JsAttributeType, ""+TreeHelper.Types.image );
					output.Attributes.Add( JsAttributePath, Cwd.Pwd() );
					output.Attributes.Add( AttributeParameters, Parms );
				}
			}
			logHelper.AddLogMessage( $"{nameof(ImageTagHelper)}: END '{Path}'" );
			return Task.FromResult(0);
		}
	}
}
