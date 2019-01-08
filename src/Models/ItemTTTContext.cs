using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ItemTTT.Models
{
	public class ItemTTTContext : DbContext
	{
		internal static readonly Dictionary<string,string> KnownErrorTriggers = new Dictionary<string, string>{
																						{ "'IX_Car'", "An item with the same code already exists" },
																					};

		public DbSet<Configuration>		Configurations		{ get; set; }
		public DbSet<Translation>		Translations		{ get; set; }
		public DbSet<Item>				Items				{ get; set; }
		public DbSet<ItemPicture>		ItemPictures		{ get; set; }
		public DbSet<BlogPost>			BlogPosts			{ get; set; }

		public ItemTTTContext(DbContextOptions options) : base(options)  {}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if( Utils.IsDebug )
			{
				// Performance: do NOT try to evaluate queries client-side. Throw an exception instead of showing a warning.
				// Cf. https://docs.microsoft.com/en-us/ef/core/querying/client-eval
				optionsBuilder.ConfigureWarnings( v=>v.Throw(RelationalEventId.QueryClientEvaluationWarning) );
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Item>()
							.HasIndex( v=>v.Code )
							.IsUnique();

			modelBuilder.Entity<ItemPicture>()
							.HasOne( v=>v.Item )
							.WithMany( v=>v.Pictures )
							.HasForeignKey( v=>v.ItemID )
							.OnDelete( DeleteBehavior.Cascade );
			modelBuilder.Entity<ItemPicture>()
							.HasIndex( v=>new{ v.ItemID, v.Number, v.Type } )
							.IsUnique();

			modelBuilder.Entity<Translation>()
							.HasIndex( v=>new{ v.TypeString, v.TranslationEN } )
							.IsUnique();
		}
	}
}
