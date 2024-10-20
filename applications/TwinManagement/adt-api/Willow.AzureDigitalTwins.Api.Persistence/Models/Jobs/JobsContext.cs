using Microsoft.EntityFrameworkCore;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

public class JobsContext : DbContext
{
    public JobsContext() { }

    public JobsContext(DbContextOptions<JobsContext> options) : base(options) { }

    public DbSet<JobsEntry> JobEntries { get; set; }

    public DbSet<JobsEntryDetail> JobEntryDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TableSplitting
        modelBuilder.Entity<JobsEntryDetail>(
            dob =>
            {
                dob.ToTable("JobEntries");
            });

        modelBuilder.Entity<JobsEntry>(
            ob =>
            {
                ob.ToTable("JobEntries");
                ob.HasOne(o => o.JobsEntryDetail).WithOne()
                    .HasForeignKey<JobsEntryDetail>(o => o.JobId);
            });
        #endregion
    }
}
