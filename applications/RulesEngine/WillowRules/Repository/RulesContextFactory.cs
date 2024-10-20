using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Willow.Rules.Repository;

/// <summary>
/// Special class used by Entity Framework when generating migrations
/// </summary>
public class RulesContextFactory : IDesignTimeDbContextFactory<RulesContext>
{
	public RulesContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<RulesContext>();
		optionsBuilder.UseSqlServer("Server=localhost,1433; Database=wil-prd-lda-brf; User=sa; Password=Willow3163412;",
			options => { options.MigrationsAssembly("WillowRules"); });

		return new RulesContext(optionsBuilder.Options);
	}
}
