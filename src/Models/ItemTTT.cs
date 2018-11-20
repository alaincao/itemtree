using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ItemTTT.Models
{
	public class ItemTTT : DbContext
	{
		public ItemTTT(DbContextOptions options) : base(options)  {}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// Do NOT try to evaluate queries client-side. Throw an exception instead of showing a warning.
			// Cf. https://docs.microsoft.com/en-us/ef/core/querying/client-eval
			optionsBuilder.ConfigureWarnings( w=>w.Throw(RelationalEventId.QueryClientEvaluationWarning) );
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}
	}
}
