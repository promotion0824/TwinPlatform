using Microsoft.EntityFrameworkCore;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;

public class MappingContext : DbContext
{
    public MappingContext() { }

    public MappingContext(DbContextOptions<MappingContext> options) : base(options) { }

    public DbSet<MappedEntry> MappedEntries { get; set; }

    public DbSet<UpdateMappedTwinRequest> UpdateMappedTwinRequest { get; set; }
}
