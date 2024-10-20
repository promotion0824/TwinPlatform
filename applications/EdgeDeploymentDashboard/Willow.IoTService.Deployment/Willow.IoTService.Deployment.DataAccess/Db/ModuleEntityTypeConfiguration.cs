using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class ModuleEntityTypeConfiguration : IEntityTypeConfiguration<ModuleEntity>
{
    public void Configure(EntityTypeBuilder<ModuleEntity> builder)
    {
        builder.Property(x => x.Id)
               .IsRequired();
        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.ModuleType)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.IsArchived)
               .IsRequired();
        builder.Property(x => x.IsSynced)
               .HasDefaultValue(true);
        builder.ConfigureBaseEntity();

        builder.HasKey(x => x.Id);
    }
}
