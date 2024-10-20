using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class ModuleConfigEntityTypeConfiguration : IEntityTypeConfiguration<ModuleConfigEntity>
{
    public void Configure(EntityTypeBuilder<ModuleConfigEntity> builder)
    {
        builder.Property(x => x.IsAutoDeployment)
               .IsRequired();
        builder.Property(x => x.DeviceName)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.IoTHubName)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.Environment)
               .IsRequired();
        builder.Property(x => x.Platform)
               .HasConversion(x => x.ToString(), x => Enum.Parse<Platforms>(x))
               .HasDefaultValueSql($"'{Platforms.arm64v8.ToString()}'")
               .HasMaxLength(VarCharPropertyLengths.Short);
        builder.ConfigureBaseEntity();

        builder.HasKey(x => x.ModuleId);

        builder.HasOne(x => x.Module)
               .WithOne(x => x.Config)
               .HasForeignKey<ModuleConfigEntity>(x => x.ModuleId)
               .IsRequired();
    }
}
