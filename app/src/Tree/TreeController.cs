
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Tree
{
	using Metadata = IDictionary<string,object>;

	public class TreeController : Views.BaseController
	{
		private readonly PageHelper	PageHelper;
		private readonly Cwd		Cwd;

		public TreeController(PageHelper pageHelper, Cwd cwd)
		{
			PageHelper = pageHelper;
			Cwd = cwd;
		}

		[HttpPost( Routes.TreeOperations )]
		public async Task<Utils.TTTServiceResult<List<object>>> Operations([FromBody]List<OperationsDTO> operations)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				if( operations == null )
					throw new ArgumentException( $"Missing parameter '{nameof(operations)}" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"{nameof(Operations)}: START" );
				var rv = new List<object>();

				logHelper.AddLogMessage( $"{nameof(Operations)}: Create transaction" );
				using( var transaction = await Cwd.BeginTransaction() )
				{
					for( var i=0; i<operations.Count; ++i )
					{
						logHelper.AddLogMessage( $"{nameof(Operations)}: Operation {i+1} of {operations.Count}" );
						var operation = operations[ i ];
						if( operation.GetNodeData != null )
						{
							var op = operation.GetNodeData;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetNodeData)} ; Path:'{op.Path}'" );
							using( Cwd.PushDisposable(op.Path) )
							{
								var data = await Cwd.TreeHelper.GetNodeData( Cwd );

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetNodeData)} 'path':'{path}' ; data length:'{""+data?.Length}'" );
								rv.Add( new{ Path=path, Data=data } );
							}
						}
						else if( operation.SetNodeData != null )
						{
							var op = operation.SetNodeData;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.SetNodeData)} ; Path:'{op.Path}' ; ExpectedType:'{op.ExpectedType}'" );

							var data = op.Data as string;
							if( data == null )
								data = op.Data.ToString();

							var expectedType = (TreeHelper.Types?)null;
							if( op.ExpectedType != null )
							{
								expectedType = op.ExpectedType.TryParseEnum<TreeHelper.Types>();
								if( expectedType == null )
									throw new ArgumentException( $"Unknown expected type '{op.ExpectedType}'" );
							}

							using( Cwd.PushDisposable(op.Path) )
							{
								await Cwd.TreeHelper.SetNodeData( Cwd, data, expectedType );

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.SetNodeData)} '{path}'" );
								rv.Add( new{ Path=path } );
							}
						}
						else if( operation.GetOrCreateNode != null )
						{
							var op = operation.GetOrCreateNode;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetOrCreateNode)} ; Path:'{op.Path}' ; ExpectedType:'{op.ExpectedType}'" );

							var expectedType = (TreeHelper.Types?)null;
							if( op.ExpectedType != null )
							{
								expectedType = op.ExpectedType.TryParseEnum<TreeHelper.Types>();
								if( expectedType == null )
									throw new ArgumentException( $"Unknown expected type '{op.ExpectedType}'" );
							}

							string data;
							if( op.Data == null )
								data = null;
							else
								data = op.Data.ToString();

							using( Cwd.PushDisposable(op.Path) )
							{
								var node = await Cwd.TreeHelper.GetOrCreateNode( Cwd, expectedType, data );

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetOrCreateNode)} '{path}'" );
								rv.Add( new{ Path=path } );
							}
						}
						else
						{
							throw new NotImplementedException( "Received unknown operation" );
						}
					}

					logHelper.AddLogMessage( $"{nameof(Operations)}: CommitTransaction" );
					await Cwd.CommitTransaction( transaction );
				}

				logHelper.AddLogMessage( $"{nameof(Operations)}: END" );
				return new Utils.TTTServiceResult<List<object>>( PageHelper ){ Result=rv };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<List<object>>.LogAndNew( PageHelper, ex );
			}
		}
		public class OperationsDTO
		{
			public GetNodeDataDTO		GetNodeData			{ get; set; } = null;
			public SetNodeDataDTO		SetNodeData			{ get; set; } = null;
			public GetOrCreateNodeDTO	GetOrCreateNode		{ get; set; } = null;
			public class GetNodeDataDTO
			{
				public string	Path			{ get; set; }
			}
			public class SetNodeDataDTO
			{
				public string	Path			{ get; set; }
				public string	ExpectedType	{ get; set; }
				public object	Data			{ get; set; }
			}
			public class GetOrCreateNodeDTO
			{
				public string	Path			{ get; set; }
				public string	ExpectedType	{ get; set; }
				public object	Data			{ get; set; }
			}
		}

		[HttpGet( Routes.TreeView )]
		public async Task<IActionResult> ViewRender(string path)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(ViewRender)}: START '{Cwd.Pwd()}'" );

			var node = await Cwd.TreeHelper.GetNode( Cwd, expectedType:TreeHelper.Types.view );
			if( node == null )
				return ObjectNotFound();

			logHelper.AddLogMessage( $"{nameof(ViewRender)}: Get view's name" );
			string view;
			{
				view = node.A.TreeMetadata_ViewName();
				if( string.IsNullOrWhiteSpace(view) )
					throw NewUnexpectedException( $"View not defined in node '{Cwd.Pwd()}'" );
			}

			if( node.B.Data.Length > 0 )
			{
				logHelper.AddLogMessage( $"{nameof(ViewRender)}: Fill ViewData using node's data" );
				var dict = node.B.Data.JSONDeserialize();
				foreach( var pair in dict )
					ViewData[ pair.Key ] = pair.Value;
			}

			logHelper.AddLogMessage( $"{nameof(ViewRender)}: Render using view '{view}'" );
			return View( view );
		}

		[HttpGet( Routes.TreeHtml )]
		public async Task<IActionResult> HtmlGet(string path)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(HtmlGet)}: START '{Cwd.Pwd()}'" );

			var node = await Cwd.TreeHelper.GetNode( Cwd, expectedType:TreeHelper.Types.html );
			if( node == null )
				return ObjectNotFound();
			var data = node.B.Data;

			var contentType = node.A.TreeMetadata_ContentType() ?? "text/html; charset=utf-8";
			logHelper.AddLogMessage( $"{nameof(HtmlGet)}: Output result ({data.Length} chars) as '{contentType}'" );
			return Content( data, contentType );
		}

		[HttpGet( Routes.TreeHtmlTranslated )]
		public async Task<IActionResult> HtmlTranslatedGet(string path)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(HtmlTranslatedGet)}: START '{Cwd.Pwd()}'" );

			var node = await Cwd.TreeHelper.GetNode( Cwd, expectedType:TreeHelper.Types.translatedHtml );
			if( node == null )
				return ObjectNotFound();
			var json = node.B.Data;

			var data = GetTranslatedNodeText( PageHelper, json );

			var contentType = node.A.TreeMetadata_ContentType() ?? "text/html; charset=utf-8";
			logHelper.AddLogMessage( $"{nameof(HtmlTranslatedGet)}: Output result ({data.Length}) as '{contentType}'" );
			return Content( data, contentType );
		}

		internal static string GetTranslatedNodeText(PageHelper pageHelper, string json)
		{
			var logHelper = pageHelper.ScopeLogs;

			logHelper.AddLogMessage( $"{nameof(GetTranslatedNodeText)}: Parse data JSON" );
			var dict = json.JSONDeserialize();

			logHelper.AddLogMessage( $"{nameof(GetTranslatedNodeText)}: Try get language '{pageHelper.CurrentLanguage}'" );
			var data = (string)dict.TryGet( ""+pageHelper.CurrentLanguage );
			if( data == null )
			{
				logHelper.AddLogMessage( $"{nameof(GetTranslatedNodeText)}: Try get language '{Language.Default}'" );
				data = (string)dict.TryGet( ""+Language.Default );
				if( data == null )
				{
					logHelper.AddLogMessage( $"{nameof(GetTranslatedNodeText)}: No matching data found => Returning an empty string" );
					data = "";
				}
			}

			return data;
		}

		[HttpGet( Routes.TreeImage )]
		public async Task<IActionResult> ImageGet(string path, int? height=null, bool forDownload=false)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(ImageGet)}: START '{Cwd.Pwd()}'" );

			int nodeID;
			Metadata metadata;
			{
				var item = await Cwd.TreeHelper.GetNodeMetadata( Cwd, expectedType:TreeHelper.Types.image );
				if( item == null )
					return ObjectNotFound();
				nodeID = item.A;
				metadata = item.B;
			}

			byte[] imgBytes;
			if( height == null )
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Request original size" );

				var imgStr = await Cwd.DataContext.TreeNodes.Where( v=>v.ID == nodeID ).Select( v=>v.Data ).SingleAsync();
				imgBytes = Convert.FromBase64String( imgStr );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Request height '{height}'" );
				string resizedStr;
				var resizedNodeName = $"height_{height.Value}";
				using( Cwd.PushDisposable( resizedNodeName ) )
				{
					resizedStr = await Cwd.TreeHelper.GetNodeData( Cwd );
				}

				if( resizedStr != null )
				{
					logHelper.AddLogMessage( $"{nameof(ImageGet)}: Requested height already available" );
					imgBytes = Convert.FromBase64String( resizedStr );
				}
				else
				{
					logHelper.AddLogMessage( $"{nameof(ImageGet)}: Create resized image" );
					var originalStr = await Cwd.DataContext.TreeNodes.Where( v=>v.ID == nodeID ).Select( v=>v.Data ).SingleAsync();
					var originalBytes = Convert.FromBase64String( originalStr );
					using( var originalStream = new System.IO.MemoryStream( originalBytes ) )
					{
						var originalBitmap = new System.Drawing.Bitmap( originalStream );
						using( var resizedStream = new System.IO.MemoryStream() )
						{
							var scaleHToW = Services.ItemPictureController.ScaleHToW;  // TODO: Manage resize/crop(?)/ratio
							var scaleWToH = Services.ItemPictureController.ScaleWToH;
							var newImage = Services.ItemPictureController.CropResize( originalBitmap, destinationHeight:(double)height.Value, scaleHToW:scaleHToW, scaleWToH:scaleWToH );
							newImage.Save( resizedStream, System.Drawing.Imaging.ImageFormat.Jpeg );
							imgBytes = resizedStream.ToArray();
						}
					}
					resizedStr = Convert.ToBase64String( imgBytes );

					logHelper.AddLogMessage( $"{nameof(ImageGet)}: Save resized image data" );
					using( var transaction = await Cwd.BeginTransaction() )
					using( Cwd.PushDisposable(resizedNodeName) )
					{
						await Cwd.TreeHelper.GetOrCreateNode( Cwd, data:resizedStr );
						await Cwd.CommitTransaction( transaction );
					}
				}
			}

			logHelper.AddLogMessage( $"{nameof(ImageGet)}: Parse image content" );
			Services.ItemPictureController.GetImageType( imgBytes, out var contentType, out var fileExtension );

			if( forDownload )
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Get file name'" );
				var fileName = metadata.TreeMetadata_FileName() ?? ("image"+fileExtension);

				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Output using contentType:'{contentType}' ; fileName:'{fileName}'" );
				return File( new System.IO.MemoryStream(imgBytes), contentType, fileName );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Output using contentType:'{contentType}'" );
				return File( new System.IO.MemoryStream(imgBytes), contentType );
			}
		}

		[HttpPost( Routes.TreeImage )]
		public async Task<Utils.TTTServiceResult> ImageSave(string path, Microsoft.AspNetCore.Http.IFormFile file)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				var logHelper = PageHelper.ScopeLogs;
				path = path ?? Cwd.Pwd();
				logHelper.AddLogMessage( $"{nameof(ImageSave)}: START '{path}'" );

				var imageData = await Services.ItemPictureController.GetImageBase64String( logHelper, file );

				string name, dir;
				using( var transaction = await Cwd.BeginTransaction() )
				using( Cwd.PushDisposable(path) )
				{
					logHelper.AddLogMessage( $"{nameof(ImageSave)}: Delete any resized images" );
					await Cwd.TreeHelper.DelTree( Cwd, included:false );

					var baseNode = await Cwd.TreeHelper.GetOrCreateNode( Cwd, expectedType:TreeHelper.Types.image, data:imageData );

					logHelper.AddLogMessage( $"{nameof(ImageSave)}: Add file name '{file.FileName}' to node's meta data" );
					var meta = baseNode.Meta.JSONDeserialize();
					meta.TreeMetadata_FileName( file.FileName );
					baseNode.Meta = meta.JSONStringify();
					await Cwd.DataContext.SaveChangesAsync();

					await Cwd.CommitTransaction( transaction );

					path = Cwd.Pwd();
					dir = Cwd.PwdParent();
					name = path.Substring( dir.Length );
				}

				logHelper.AddLogMessage( $"{nameof(ImageSave)}: END '{path}'" );
				return Utils.TTTServiceResult.New( PageHelper, new{ Name=name, Dir=dir, Path=path } );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.TreeDownload )]
		public async Task TreeDownload(string path)
		{
			var outputStarted = false;
			try
			{
				Response.ContentType = "text/plain";
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				if( string.IsNullOrWhiteSpace(path) )
					throw new Utils.TTTException( $"Missing parameter '{nameof(path)}'" );
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"{nameof(TreeDownload)}: START '{path}'" );
				Cwd.Cd( path );

				var eol = System.Text.Encoding.UTF8.GetBytes( "\n" );
				await foreach( var line in TreeHelper.SaveTree(Cwd) )
				{
					var bytes = System.Text.Encoding.UTF8.GetBytes( line );
					await Response.Body.WriteAsync( bytes );
					await Response.Body.WriteAsync( eol );
					outputStarted = true;
				}
			}
			catch( System.Exception ex )
			{
				if(! outputStarted )
				{
					Response.StatusCode = 500;
				}
				else
				{
					// Too late to change status code :-(
					var errBytes = System.Text.Encoding.UTF8.GetBytes( $"\n\n*** ERROR ***\n\n" );  // nb: make a clear visual separation ...
					await Response.Body.WriteAsync( errBytes );
				}
				var response = Utils.TTTServiceResult.LogAndNew( PageHelper, ex ).JSONStringify();
				var bytes = System.Text.Encoding.UTF8.GetBytes( response );
				await Response.Body.WriteAsync( bytes );
			}
			await Response.Body.FlushAsync();
			Response.Body.Close();
		}
	}

	internal static partial class ExtensionMethods
	{
		internal static string TreeMetadata_ViewName(this Metadata metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_ViewName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "view" );
		}

		internal static string TreeMetadata_ContentType(this Metadata metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "contentType" );
		}

		internal static string TreeMetadata_FileName(this Metadata metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "fileName" );
		}
		internal static void TreeMetadata_FileName(this Metadata metadata, string fileName)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			if( string.IsNullOrWhiteSpace(fileName) )
				metadata.Remove( "fileName" );
			else
				metadata[ "fileName" ] = fileName.Trim();
		}
	}
}
