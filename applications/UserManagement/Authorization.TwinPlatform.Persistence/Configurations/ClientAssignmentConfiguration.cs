using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Client Assignment Table Configuration.
/// </summary>
internal class ClientAssignmentConfiguration : IEntityTypeConfiguration<ClientAssignment>
{
    /// <summary>
    /// Configure Client Assignment Entity Configuration
    /// </summary>
    /// <param name="builder">Entity Builder instance</param>
    public void Configure(EntityTypeBuilder<ClientAssignment> builder)
    {
        builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
        builder.HasMany(m=>m.ClientAssignmentPermissions).WithOne(o=>o.ClientAssignment).HasForeignKey(o=>o.ClientAssignmentId);
        builder.HasIndex(x => new {x.ApplicationClientId,x.Expression}).IsUnique();
    }
}
