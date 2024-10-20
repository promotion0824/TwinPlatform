using InsightCore.Models;
using Microsoft.EntityFrameworkCore;

namespace InsightCore.Entities
{
    public class InsightDbContext : DbContext
    {
        public InsightDbContext(DbContextOptions<InsightDbContext> options) : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<InsightEntity> Insights { get; set; }
        public DbSet<ImpactScoreEntity> ImpactScores { get; set; }
		public DbSet<InsightOccurrenceEntity> InsightOccurrences { get; set; }


		public DbSet<InsightNextNumberEntity> InsightNextNumbers { get; set; }
		public DbSet<StatusLogEntity> StatusLog { get; set; }
		public DbSet<DependencyEntity> Dependencies { get; set; }
        public DbSet<InsightLocationEntity> InsightLocations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<InsightNextNumberEntity>().HasKey(i => i.Prefix);
			modelBuilder.Entity<InsightEntity>().HasQueryFilter(x => x.Status != InsightStatus.Deleted && x.OccurrenceCount > 0);
		}
    }
}
