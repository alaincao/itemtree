using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ItemTTT.Tree
{
	public abstract class TagHelperBase : TagHelper
	{
		protected const string	CsTagPageProperty			= "tree-pageProperty";
		protected const string	CsAttributeHtml				= "tree-html";
		protected const string	CsAttributeHtmlTranslated	= "tree-htmlTranslated";
		protected const string	CsAttributeFile				= "tree-file";
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

	[HtmlTargetElement(Attributes = TagHelperBase.CsAttributeFile)]
	public class FileTagHelper : TagHelperBase
	{
		[HtmlAttributeName(TagHelperBase.CsAttributeFile)]
		public string		Path		{ get; set; }

		public FileTagHelper(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd) : base(pageHelper, dataContext, cwd)  {}

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(FileTagHelper)}: Start '{Path}'" );

			using( Cwd.PushDisposable(Path) )
			{
				var src = Cwd.Pwd();

				if( PageHelper.IsAuthenticated )
				{
					output.Attributes.Add( JsAttributeType, ""+TreeHelper.Types.file );
					output.Attributes.Add( JsAttributePath, Cwd.Pwd() );
				}
			}
			logHelper.AddLogMessage( $"{nameof(FileTagHelper)}: END '{Path}'" );
			return Task.FromResult(0);
		}
	}

	[HtmlTargetElement(Attributes = TagHelperBase.CsAttributeImage)]
	public class ImageTagHelper : TagHelperBase
	{
		internal const double	ScaleHToW	= (double)1920/1200;  // 1.6
		internal const double	ScaleWToH	= (double)1200/1920;  // 0.625

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

	[HtmlTargetElement(TagHelperBase.CsTagPageProperty, Attributes = CsAttributeProperty)]
	public class PagePropertyTagHelper : TagHelperBase
	{
		private const string	CsAttributePath			= "path";
		private const string	CsAttributeProperty		= "property";
		private const string	JsAttributeProperty		= "property";

		[HtmlAttributeName(CsAttributePath)]
		public string		Path		{ get; set; }

		[HtmlAttributeName(CsAttributeProperty)]
		public string		Property	{ get; set; }

		public PagePropertyTagHelper(PageHelper pageHelper, Models.ItemTTTContext dataContext, Tree.Cwd cwd) : base(pageHelper, dataContext, cwd)  {}

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(PagePropertyTagHelper)}: Start '{Path}'" );

			if(! PageHelper.IsAuthenticated )
			{
				// Simply discard output
				output.SuppressOutput();
				goto EXIT;
			}

			output.TagName = "script";
			output.Attributes.Add( "type", "text/html" );
			output.Attributes.Add( JsAttributeType, ""+TreeHelper.Types.pageProperty );
			using( Cwd.PushDisposable(Path??".") )
				output.Attributes.Add( JsAttributePath, Cwd.Pwd() );
			output.Attributes.Add( JsAttributeProperty, Property );

		EXIT:
			logHelper.AddLogMessage( $"{nameof(PagePropertyTagHelper)}: END '{Path}'" );
			return Task.FromResult(0);
		}
	}
}
