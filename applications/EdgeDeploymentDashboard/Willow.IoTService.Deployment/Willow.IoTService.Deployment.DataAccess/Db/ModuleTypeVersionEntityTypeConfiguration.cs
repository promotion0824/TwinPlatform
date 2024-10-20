using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class ModuleTypeVersionEntityTypeConfiguration : IEntityTypeConfiguration<ModuleTypeVersionEntity>
{
    public void Configure(EntityTypeBuilder<ModuleTypeVersionEntity> builder)
    {
        builder.Property(x => x.ModuleType)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.ConfigureBaseEntity();

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ModuleType);
    }
}
