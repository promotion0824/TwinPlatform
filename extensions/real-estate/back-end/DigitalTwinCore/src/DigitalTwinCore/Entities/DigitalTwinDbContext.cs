using Microsoft.EntityFrameworkCore;

namespace DigitalTwinCore.Entities
{
    public class DigitalTwinDbContext : DbContext
    {
        private readonly DbContextOptions<DigitalTwinDbContext> _options;
        public DbContextOptions<DigitalTwinDbContext> Options => _options;

        public DigitalTwinDbContext(DbContextOptions<DigitalTwinDbContext> options) : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _options = options;
        }

        public DbSet<SiteSettingEntity> SiteSettings { get; set; }

        public DbSet<TagEntity> Tags { get; set; }

        public DbSet<SiteVirtualTagEntity> VirtualTags { get; set; }

        public DbSet<GeometryViewerModelEntity> GeometryViewerModels { get; set; }

        public DbSet<GeometryViewerReferenceEntity> GeometryViewerReferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
