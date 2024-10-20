using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Application table entity framework configuration.
/// </summary>
internal class ApplicationClientConfiguration : IEntityTypeConfiguration<ApplicationClient>
{
    /// <summary>
    /// Configure Application Entity Configuration
    /// </summary>
    /// <param name="builder">Entity Builder instance</param>
    public void Configure(EntityTypeBuilder<ApplicationClient> builder)
    {
        builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasOne(o=>o.Application).WithMany(m=>m.Clients).HasForeignKey(x=>x.ApplicationId);
    }
}
