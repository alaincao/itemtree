
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace ItemTTT.Models
{
	public class ItemTTTContext : DbContext
	{
		internal static readonly Dictionary<string,string> KnownErrorTriggers = new Dictionary<string, string>{
																						{ "'IX_Item'", "An item with the same code already exists" },
																					};

		public DbSet<TreeNode>			TreeNodes			{ get; set; }

		public ItemTTTContext(DbContextOptions options) : base(options)  {}

		/// <remarks>Constructor for specific uses</remarks>
		internal static ItemTTTContext New(string connectionString)
		{
			var optionsBuilder = new DbContextOptionsBuilder<Models.ItemTTTContext>()
											.UseSqlServer( connectionString );
			return new Models.ItemTTTContext( optionsBuilder.Options );
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TreeNode>()
							.HasIndex( v=>new{ v.Path } )
							.IsUnique();
			modelBuilder.Entity<TreeNode>()
							.HasIndex( v=>new{ v.ParentPath, v.Meta } );
		}
	}
}
