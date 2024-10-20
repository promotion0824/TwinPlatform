using AssetCoreTwinCreator.MappingId.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetCoreTwinCreator.MappingId
{
    public class MappingDbContext : DbContext
    {
        public MappingDbContext(DbContextOptions<MappingDbContext> options) : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<SiteMapping> SiteMappings { get; set; }

        public DbSet<FloorMapping> FloorMappings { get; set; }

        public DbSet<AssetGeometry> AssetGeometries { get; set; }

        public DbSet<AssetEquipmentMapping> AssetEquipmentMappings { get; set; }

        public DbSet<AssetCategoryExtensionEntity> AssetCategoryExtensions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SiteMapping>(entity => { entity.HasAlternateKey(e => e.BuildingId); });

            modelBuilder.Entity<FloorMapping>(entity => { entity.HasAlternateKey(e => e.FloorCode); });

            modelBuilder.Entity<AssetCategoryExtensionEntity>().HasKey(e => new { e.SiteId, e.CategoryId });
        }
    }
}
