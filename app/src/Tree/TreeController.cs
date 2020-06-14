
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Tree
{
	using MetaData = IDictionary<string,object>;

	public class TreeController : Views.BaseController
	{
		private readonly PageHelper	PageHelper;
		private readonly Cwd		Cwd;

		public TreeController(PageHelper pageHelper, Cwd cwd)
		{
			PageHelper = pageHelper;
			Cwd = cwd;
		}

		[HttpGet( Routes.TreeSanitizePath )]
		[HttpPost( Routes.TreeSanitizePath )]
		public string SanitizePath(string path)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(SanitizePath)}: START '{path}'" );
			if( string.IsNullOrWhiteSpace(path) )
				return "";
			return Cwd.SanitizePath( path );
		}

		[HttpGet( Routes.TreeSanitizeName )]
		[HttpPost( Routes.TreeSanitizeName )]
		public string SanitizeName(string name)
		{
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(SanitizeName)}: START '{name}'" );
			if( string.IsNullOrWhiteSpace(name) )
				return "";
			return Cwd.SanitizeName( name );
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

						if( operation.GetChildNodes != null )
						{
							var op = operation.GetChildNodes;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetChildNodes)} ; Path:'{op.Path}'" );
							using( Cwd.PushDisposable(op.Path) )
							{
								var childNodes = (await Cwd.TreeHelper.GetChildNodes( Cwd )).Select( v=>v.B ).ToArray();

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetChildNodes)} Path:'{path}' ; length:'{childNodes.Length}'" );
								rv.Add( new{ Path=path, ChildNodes=childNodes } );
							}
						}
						else if( operation.GetNodeMetaData != null )
						{
							var op = operation.GetNodeMetaData;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetNodeMetaData)} ; Path:'{op.Path}'" );
							using( Cwd.PushDisposable(op.Path) )
							{
								(await Cwd.TreeHelper.GetNodeMetaData( Cwd )).R( out var _, out var meta );

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.GetNodeMetaData)} 'path':'{path}'" );
								rv.Add( new{ Path=path, MetaData=meta } );
							}
						}
						else if( operation.GetNodeData != null )
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
						else if( operation.SetNodeMetaData != null )
						{
							var op = operation.SetNodeMetaData;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.SetNodeMetaData)} ; Path:'{op.Path}'" );

							var strMeta = op.MetaData as string;
							if( strMeta == null )
								strMeta = op.MetaData.ToString();
							var meta = strMeta.JSONDeserialize();

							var expectedType = (TreeHelper.Types?)null;
							if( op.ExpectedType != null )
							{
								expectedType = op.ExpectedType.TryParseEnum<TreeHelper.Types>();
								if( expectedType == null )
									throw new ArgumentException( $"Unknown expected type '{op.ExpectedType}'" );
							}

							using( Cwd.PushDisposable(op.Path) )
							{
								await Cwd.TreeHelper.SetNodeMetaData( Cwd, meta, expectedType );

								var path = Cwd.Pwd();
								logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.SetNodeMetaData)} '{path}'" );
								rv.Add( new{ Path=path } );
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
						else if( operation.DelTree != null )
						{
							var op = operation.DelTree;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.DelTree)} ; Path:'{op.Path}' ; Included:'{op.Included}'" );

							using( Cwd.PushDisposable(op.Path) )
							{
								var c = await Cwd.TreeHelper.DelTree( Cwd, included:op.Included??true );
								rv.Add( new{ Path=Cwd.Pwd(), AffectedRows=c } );
							}
						}
						else if( operation.RestoreTree != null )
						{
							var op = operation.RestoreTree;
							logHelper.AddLogMessage( $"{nameof(Operations)}: {nameof(operation.RestoreTree)} ; Path:'{op.Path}' ; FilePath:'{op.FilePath}'" );

							using( Cwd.PushDisposable(op.Path) )
							{
								await Cwd.TreeHelper.RestoreTree( Cwd, op.FilePath, createTransaction:false, overwrite:op.Overwrite??false );
								rv.Add( new{ Path=Cwd.Pwd() } );
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
			public GetChildNodesDTO		GetChildNodes		{ get; set; } = null;
			public GetNodeMetaDataDTO	GetNodeMetaData		{ get; set; } = null;
			public GetNodeDataDTO		GetNodeData			{ get; set; } = null;
			public SetNodeMetaDataDTO	SetNodeMetaData		{ get; set; } = null;
			public SetNodeDataDTO		SetNodeData			{ get; set; } = null;
			public GetOrCreateNodeDTO	GetOrCreateNode		{ get; set; } = null;
			public DelTreeDTO			DelTree				{ get; set; } = null;
			public RestoreTreeDTO		RestoreTree			{ get; set; } = null;
			public class GetChildNodesDTO
			{
				public string	Path			{ get; set; }
			}
			public class GetNodeMetaDataDTO
			{
				public string	Path			{ get; set; }
			}
			public class GetNodeDataDTO
			{
				public string	Path			{ get; set; }
			}
			public class SetNodeMetaDataDTO
			{
				public string	Path			{ get; set; }
				public string	ExpectedType	{ get; set; }
				public object	MetaData		{ get; set; }
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
			public class DelTreeDTO
			{
				public string	Path			{ get; set; }
				public bool?	Included		{ get; set; }
			}
			public class RestoreTreeDTO
			{
				public string	Path			{ get; set; }
				public string	FilePath		{ get; set; }
				public bool?	Overwrite		{ get; set; }
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

		[HttpPost( Routes.TreeFile )]
		public async Task<Utils.TTTServiceResult> FileSave(string path, Microsoft.AspNetCore.Http.IFormFile file, string contentType=null)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				var logHelper = PageHelper.ScopeLogs;
				path = path ?? Cwd.Pwd();
				logHelper.AddLogMessage( $"{nameof(FileSave)}: START '{path}'" );

				var fileName = file.FileName;
				if( contentType == null )
				{
					logHelper.AddLogMessage( $"{nameof(FileSave)}: Get content type from file name '{fileName}'" );
					contentType = Utils.Files.GetContentType( fileName );
				}
				logHelper.AddLogMessage( $"{nameof(FileSave)}: File name: '{fileName}' ; Content type: '{contentType}'" );

				(await Utils.Files.GetBase64String( logHelper, file )).R( out var contentBase64, out var _ );

				string name, dir;
				using( var transaction = await Cwd.BeginTransaction() )
				using( Cwd.PushDisposable(path) )
				{
					var baseNode = await Cwd.TreeHelper.GetOrCreateNode( Cwd, expectedType:TreeHelper.Types.file, data:contentBase64 );

					logHelper.AddLogMessage( $"{nameof(FileSave)}: Add file name & content type to node's meta data" );
					var meta = baseNode.Meta.JSONDeserialize();
					meta.TreeMetadata_FileName( fileName );
					meta.TreeMetadata_ContentType( contentType );
					baseNode.Meta = meta.JSONStringify();
					await Cwd.DataContext.SaveChangesAsync();

					await Cwd.CommitTransaction( transaction );

					path = Cwd.Pwd();
					dir = Cwd.PwdParent();
					name = path.Substring( dir.Length );
					contentType = meta.TreeMetadata_ContentType();  // nb: Refresh from actually saved to the database ...
				}

				logHelper.AddLogMessage( $"{nameof(FileSave)}: END '{path}'" );
				return Utils.TTTServiceResult.New( PageHelper, new{ Name=name, Dir=dir, Path=path, FileName=fileName, ContentType=contentType } );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}

		[HttpGet( Routes.TreeFile )]
		public async Task<IActionResult> FileGet(string path, bool forDownload=true)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(FileGet)}: START '{Cwd.Pwd()}'" );

			int nodeID;
			MetaData metadata;
			{
				var item = await Cwd.TreeHelper.GetNodeMetaData( Cwd, expectedType:TreeHelper.Types.file );
				if( item == null )
					return ObjectNotFound();
				nodeID = item.A;
				metadata = item.B;
			}

			logHelper.AddLogMessage( $"{nameof(FileGet)}: Get file data" );
			var fileStr = await TreeHelper.GetNodeDataFast( Cwd.DataContext, nodeID:nodeID );
			var fileBytes = Convert.FromBase64String( fileStr );

			var contentType = metadata.TreeMetadata_ContentType();
			if( forDownload )
			{
				logHelper.AddLogMessage( $"{nameof(FileGet)}: Get file name'" );
				var fileName = metadata.TreeMetadata_FileName();

				logHelper.AddLogMessage( $"{nameof(FileGet)}: Output using contentType:'{contentType}' ; fileName:'{fileName}'" );
				return File( new System.IO.MemoryStream(fileBytes), contentType, fileName );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(FileGet)}: Output using contentType:'{contentType}'" );
				return File( new System.IO.MemoryStream(fileBytes), contentType );
			}
		}

		[HttpGet( Routes.TreeImage )]
		public async Task<IActionResult> ImageGet(string path, int? height=null, bool forDownload=false)
		{
			if( path != null )
				Cwd.Cd( path );
			var logHelper = PageHelper.ScopeLogs;
			logHelper.AddLogMessage( $"{nameof(ImageGet)}: START '{Cwd.Pwd()}'" );

			int nodeID;
			MetaData metadata;
			{
				var item = await Cwd.TreeHelper.GetNodeMetaData( Cwd, expectedType:TreeHelper.Types.image );
				if( item == null )
					return ObjectNotFound();
				nodeID = item.A;
				metadata = item.B;
			}

			byte[] imgBytes;
			if( height == null )
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Request original size" );

				var imgStr = await TreeHelper.GetNodeDataFast( Cwd.DataContext, nodeID:nodeID );
				imgBytes = Convert.FromBase64String( imgStr );
			}
			else
			{
				logHelper.AddLogMessage( $"{nameof(ImageGet)}: Request height '{height}'" );
				string resizedStr;
				var resizedNodeName = $"height_{height.Value}";
				using( Cwd.PushDisposable( resizedNodeName ) )
				{
					resizedStr = await TreeHelper.GetNodeDataFast( Cwd.DataContext, path:Cwd.PwdDb() );
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
							var newImage = Utils.Images.CropResize( originalBitmap, destinationHeight:(double)height.Value, scaleHToW:scaleHToW, scaleWToH:scaleWToH );
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
			Utils.Images.GetImageType( imgBytes, out var contentType, out var fileExtension );

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

				var imageData = await Utils.Images.GetImageBase64String( logHelper, file );

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
		public async Task TreeDownload(string path, bool excludeImages=false, bool forDownload=false)
		{
			var outputStarted = false;
			try
			{
				if( forDownload )
					Response.ContentType = "application/octet-stream";
				else
					Response.ContentType = "text/plain";

				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				if( string.IsNullOrWhiteSpace(path) )
					throw new Utils.TTTException( $"Missing parameter '{nameof(path)}'" );
				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"{nameof(TreeDownload)}: START '{path}'" );
				Cwd.Cd( path );

				var eol = System.Text.Encoding.UTF8.GetBytes( "\n" );
				await foreach( var line in Cwd.TreeHelper.SaveTree(Cwd, excludeImages:excludeImages) )
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
		internal static string TreeMetadata_ViewName(this MetaData metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_ViewName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "view" );
		}

		internal static string TreeMetadata_ContentType(this MetaData metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "contentType" );
		}
		internal static void TreeMetadata_ContentType(this MetaData metadata, string contentType)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_ContentType), $"Missing parameter '{nameof(metadata)}'" );
			if( string.IsNullOrWhiteSpace(contentType) )
				metadata.Remove( "contentType" );
			else
				metadata[ "contentType" ] = contentType.ToLower().Trim();
		}

		internal static string TreeMetadata_FileName(this MetaData metadata)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			return (string)metadata.TryGet( "fileName" );
		}
		internal static void TreeMetadata_FileName(this MetaData metadata, string fileName)
		{
			Utils.Assert( metadata != null, nameof(TreeMetadata_FileName), $"Missing parameter '{nameof(metadata)}'" );
			if( string.IsNullOrWhiteSpace(fileName) )
				metadata.Remove( "fileName" );
			else
				metadata[ "fileName" ] = fileName.Trim();
		}
	}
}
