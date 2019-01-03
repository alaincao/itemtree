
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Services
{
	public class TranslationController : Views.BaseController
	{
		public enum Types
		{
			Undefined	= 0,
			Feature,
		}

		private readonly PageHelper				PageHelper;
		private readonly Models.ItemTTTContext	DataContext;

		public TranslationController(Models.ItemTTTContext dataContext, PageHelper pageHelper)
		{
			PageHelper = pageHelper;
			DataContext = dataContext;
		}

		[HttpGet( Routes.GetTranslations )]
		[HttpPost( Routes.GetTranslations )]
		public async Task<Utils.TTTServiceResult<DTOs.TranslationItem[]>> List(Types type, bool includeOriginals=true, bool includeTranslations=true)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					// As of now, this is only useful for admins ...
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogMessage( $"TranslationsList: START: {nameof(type)}:{type}" );

				var dc = DataContext;

				var originals = new HashSet<string>();
				if( includeOriginals )
				{
					logHelper.AddLogLines( $"TranslationsList: Using originals" );
					switch( type )
					{
						case Types.Feature: {
							foreach( var strFeatures in await dc.Items.Select( v=>v.Features ).ToArrayAsync() )
							{
								var feature = strFeatures.JSONDeserialize<string[]>();
								originals.UnionWith( feature );
							}
							break; }
						default:
							throw new NotImplementedException( $"Unknown value '{type}' for parameter {nameof(type)}" );
					}
				}

				IDictionary<string,Models.Translation> translations;
				if( includeTranslations )
				{
					logHelper.AddLogLines( $"TranslationsList: Using translations" );
					var strType = ""+type;
					translations = await dc.Translations.Where( v=>v.TypeString == strType )
														.ToDictionaryAsync( v=>v.TranslationEN, v=>v );
				}
				else
				{
					translations = new Dictionary<string,Models.Translation>();
				}

				logHelper.AddLogLines( $"TranslationsList: Create result" );
				var keys = originals.Concat( translations.Keys ).Distinct().OrderBy( v=>v );
				var items = keys.Select( en=>
									{
										var tr = translations.TryGet( en );
										var dto = (tr == null)	? new DTOs.TranslationItem( en ){ InTranslation = false }
																: new DTOs.TranslationItem( tr ){ InTranslation = true };
										dto.InOriginal = originals.Contains( en );
										return dto;
									} )
								.ToArray();
				return new Utils.TTTServiceResult<DTOs.TranslationItem[]>( PageHelper ){ Result = items };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<DTOs.TranslationItem[]>.LogAndNew( PageHelper, ex );
			}
		}

		[HttpPost( Routes.SaveTranslations )]
		public async Task<Utils.TTTServiceResult> Save([FromBody]SaveRequest request)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					throw new Utils.TTTException( "Not logged-in" );
				if( request == null )
					throw new Utils.TTTException( $"Missing parameter '{nameof(request)}'" );
				if( request.Type == Types.Undefined )
					throw new Utils.TTTException( $"Missing parameter '{nameof(request.Type)}'" );
				if( (request.Translations == null) || (request.Translations.Length == 0 ) )
					throw new Utils.TTTException( $"Missing parameter '{nameof(request.Translations)}'" );

				var logHelper = PageHelper.ScopeLogs;
				var strType = ""+request.Type;
				logHelper.AddLogMessage( $"TranslationsSave: START: {nameof(request.Type)}:{strType}" );

				logHelper.AddLogMessage( $"TranslationsSave: Open transaction" );
				var dc = DataContext;
				using( var transaction = dc.Database.BeginTransaction() )
				{
					foreach( var item in request.Translations )
					{
						item.Sanitize();
						var en = item.EN;
						if( string.IsNullOrWhiteSpace(en) )
							// Delete translation
							en = null;

						var enOriginal = item.ENOriginal;
						logHelper.AddLogMessage( $"TranslationsSave: {nameof(enOriginal)}:'{enOriginal}' ; {nameof(en)}:'{en}'" );

						if( enOriginal != null )
						{
							logHelper.AddLogMessage( $"TranslationsSave: Request existing originals for deletion" );
							var originals = await dc.Translations	.Where( v=>v.TypeString == strType )
																	.Where( v=>v.TranslationEN == enOriginal )
																	.ToArrayAsync();
							if( originals.Length == 0 )
							{
								logHelper.AddLogMessage( $"TranslationsSave: No original translations found" );
							}
							else
							{
								logHelper.AddLogMessage( $"TranslationsSave: Delete {originals.Length} rows" );
								dc.Translations.RemoveRange( originals );
								await dc.SaveChangesAsync();
							}
						}

						if( (en != null) && (en != enOriginal) )
						{
							logHelper.AddLogMessage( $"TranslationsSave: Request existing news for deletion" );
							var news = await dc.Translations	.Where( v=>v.TypeString == strType )
																.Where( v=>v.TranslationEN == en )
																.ToArrayAsync();
							if( news.Length == 0 )
							{
								logHelper.AddLogMessage( $"TranslationsSave: No conflicting translations found" );
							}
							else
							{
								logHelper.AddLogMessage( $"TranslationsSave: Delete {news.Length} rows" );
								dc.Translations.RemoveRange( news );
								await dc.SaveChangesAsync();
							}
						}

						if( en == null )
							// Deletion of this translation requested => NOOP => Continue with next item
							continue;

						logHelper.AddLogMessage( $"TranslationsSave: Add new translation row" );
						var row = new Models.Translation{ Type = request.Type };
						item.ToModel( row );
						dc.Translations.Add( row );
						await dc.SaveChangesAsync();

						if( enOriginal != en )
						{
							logHelper.AddLogMessage( $"TranslationsSave: Rename original" );
							switch( request.Type )
							{
								case Types.Feature:
									await RenameFeature( dc, enOriginal, en );
									break;
								default:
									throw new NotImplementedException( $"Unknown value '{request.Type}' for parameter {nameof(request.Type)}" );
							}
							var rc = await dc.SaveChangesAsync();
							logHelper.AddLogMessage( $"TranslationsSave: Affected: {rc}" );
						}
					}// foreach(request.Items)

					logHelper.AddLogMessage( $"TranslationsSave: Commit transaction" );
					transaction.Commit();
				}

				return new Utils.TTTServiceResult( PageHelper );
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult.LogAndNew( PageHelper, ex );
			}
		}
		public class SaveRequest
		{
			public Types				Type			{ get; set; }
			public SaveRequestItem[]	Translations	{ get; set; }
		}
		public class SaveRequestItem : DTOs.Translation
		{
			public string	ENOriginal	{ get; set; }
		}

		private async static Task RenameFeature(Models.ItemTTTContext dc, string enOriginal, string enNew)
		{
			var items = await dc.Items.Where( v=>EF.Functions.Like(v.Features, $"%{enOriginal}%") ).ToArrayAsync();
			var enOriginalLower = enOriginal.ToLower().Trim();
			foreach( var item in items )
			{
				var features = item.Features.JSONDeserialize<string[]>();
				for( var i=0; i<features.Length; ++i )
				{
					if( features[i].ToLower().Trim() == enOriginalLower )
						features[i] = enNew;
				}
				features = features.Distinct().ToArray();  // Remove any duplicates created
				item.Features = features.JSONStringify();
			}
		}

		[HttpGet( Routes.AutoComplete )]
		[HttpPost( Routes.AutoComplete )]
		public async Task<Utils.TTTServiceResult<string[]>> AutoCompleteResolve(Types type, string searchString=null, bool includeTranslations=true)
		{
			try
			{
				if(! PageHelper.IsAuthenticated )
					// As of now, this is only useful for admins ...
					throw new Utils.TTTException( "Not logged-in" );

				var logHelper = PageHelper.ScopeLogs;
				logHelper.AddLogLines( $"AutoCompleteResolve: START: {nameof(type)}:'{type}' ; {nameof(searchString)}:'{searchString}' ; {nameof(includeTranslations)}:{includeTranslations}" );
				var dc = DataContext;

				IEnumerable<string> stringsList = null;
				IEnumerable<string> jsonLists = null;
				switch( type )
				{
					case Types.Feature:
						jsonLists = await dc.Items.Select( v=>v.Features ).ToArrayAsync();
						break;
					default:
						throw new NotImplementedException( $"Unknown value '{type}' for parameter {nameof(type)}" );
				}

				if( jsonLists != null )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Using JSON lists" );

					if( stringsList == null )
						stringsList = new string[]{};
					foreach( var jsonString in jsonLists )
					{
						var strs = jsonString.JSONDeserialize<string[]>();
						stringsList = stringsList.Concat( strs );
					}
				}

				if( includeTranslations )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Using translations" );
					var strType = ""+type;
					var trans = dc.Translations.Where( v=>v.TypeString == strType ).Select( v=>v.TranslationEN );
					stringsList = stringsList.Concat( trans );
				}

				logHelper.AddLogLines( $"AutoCompleteResolve: Distinct" );
				stringsList = stringsList.Distinct();

				if(! string.IsNullOrEmpty(searchString) )
				{
					logHelper.AddLogLines( $"AutoCompleteResolve: Applying {nameof(searchString)}" );
					searchString = searchString.ToLower();
					stringsList = stringsList.Where( v=>v.ToLower().Contains(searchString) );
				}

				logHelper.AddLogLines( $"AutoCompleteResolve: END" );
				return new Utils.TTTServiceResult<string[]>( PageHelper ){ Result=stringsList.ToArray() };
			}
			catch( System.Exception ex )
			{
				return Utils.TTTServiceResult<string[]>.LogAndNew( PageHelper, ex );
			}
		}
	}
}
