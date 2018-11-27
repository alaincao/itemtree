using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ItemTTT.Models
{
	public class ItemTTTContext : DbContext
	{
		public DbSet<Item>		Items		{ get; set; }

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
		}
	}
}
