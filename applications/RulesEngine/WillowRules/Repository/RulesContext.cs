using Microsoft.EntityFrameworkCore;
using Willow.Rules.Model;
using WillowRules.Migrations;

// DBSets get populated by EF using some hidden magic
#nullable disable

namespace Willow.Rules.Repository;

/// <summary>
/// EF Core context
/// </summary>
/// <remarks>
///  dotnet ef migrations add "name of migration" --project ../WillowRules
///  dotnet tool update --global dotnet-ef
/// </remarks>
public class RulesContext : DbContext
{
	public virtual DbSet<Rule> Rules { get; set; }
	public virtual DbSet<RuleMetadata> RuleMetadatas { get; set; }
	public virtual DbSet<RuleExecution> RuleExecutions { get; set; }
	public virtual DbSet<RuleExecutionRequest> RuleExecutionRequests { get; set; }
	public virtual DbSet<RuleInstance> RuleInstances { get; set; }
	public virtual DbSet<RuleInstanceMetadata> RuleInstanceMetadatas { get; set; }
	public virtual DbSet<Insight> Insights { get; set; }
	public virtual DbSet<Progress> Progress { get; set; }
	public virtual DbSet<CalculatedPoint> CalculatedPoints { get; set; }
	public virtual DbSet<GlobalVariable> GlobalVariables { get; set; }
	public virtual DbSet<MLModel> MLModels { get; set; }
	public virtual DbSet<Command> Commands { get; set; }
	public virtual DbSet<ADTSummary> ADTSummaries { get; set; }
	public virtual DbSet<TimeSeries> TimeSeriesBuffer { get; set; }
	public virtual DbSet<TimeSeriesMapping> TimeSeriesMappings { get; set; }
	public virtual DbSet<RuleTimeSeriesMapping> RuleTimeSeriesMapping { get; set; }
	public virtual DbSet<ActorState> ActorState { get; set; }
	public virtual DbSet<ImpactScore> ImpactScores { get; set; }
	public virtual DbSet<InsightChange> InsightChanges { get; set; }
	public virtual DbSet<LogEntry> Logs { get; set; }


	// Not stored in SQL
	//public DbSet<ActorState> ActorState { get; internal set; }

	/// <summary>
	/// Creates a new <see cref="RulesContext" />
	/// </summary>
	public RulesContext(DbContextOptions<RulesContext> options) : base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		// Configured in program.cs
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<GlobalVariable>().ToTable("GlobalVariable");

		modelBuilder.Entity<Rule>().ToTable("Rule");

		modelBuilder.Entity<RuleMetadata>().ToTable("RuleMetadata");
		modelBuilder.Entity<RuleMetadata>().Property(e => e.ScanState).HasConversion<string>();

		modelBuilder.Entity<RuleExecution>().ToTable("RuleExecution");
		modelBuilder.Entity<RuleExecution>().HasIndex(e => e.RuleId);

		modelBuilder.Entity<RuleInstance>().ToTable("RuleInstance");
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.EquipmentId);
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.EquipmentName);
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.PrimaryModelId);
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.RuleId);
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.LastUpdated);
		modelBuilder.Entity<RuleInstance>().HasIndex(e => e.Status);

		modelBuilder.Entity<RuleInstanceMetadata>().ToTable("RuleInstanceMetadata");

		modelBuilder.Entity<Insight>().ToTable("Insight");
		modelBuilder.Entity<Insight>().HasIndex(e => e.EquipmentId);
		modelBuilder.Entity<Insight>().HasIndex(e => e.EquipmentName);
		modelBuilder.Entity<Insight>().HasIndex(e => e.LastUpdated);
		modelBuilder.Entity<Insight>().HasIndex(e => e.LastFaultedDate);
		modelBuilder.Entity<Insight>().HasIndex(e => e.RuleId);
		modelBuilder.Entity<Insight>().HasMany(v => v.ImpactScores).WithOne(v => v.Insight).HasForeignKey(v => v.InsightId);
		modelBuilder.Entity<Insight>().HasMany(v => v.Occurrences).WithOne(v => v.Insight).HasForeignKey(v => v.InsightId);

		modelBuilder.Entity<InsightOccurrence>().ToTable("InsightOccurrence");
		modelBuilder.Entity<InsightOccurrence>().HasIndex(e => e.InsightId);
		modelBuilder.Entity<InsightOccurrence>().HasIndex(e => e.Ended);

		modelBuilder.Entity<ImpactScore>().ToTable("InsightImpactScore");
		modelBuilder.Entity<ImpactScore>().HasIndex(e => e.InsightId);
		modelBuilder.Entity<ImpactScore>().HasIndex(e => e.Score);
		modelBuilder.Entity<ImpactScore>().HasIndex(e => e.Name);
		modelBuilder.Entity<ImpactScore>().HasIndex(e => e.BaseScore);
		modelBuilder.Entity<ImpactScore>().HasIndex(e => e.LastUpdated);

		modelBuilder.Entity<Progress>().ToTable("Progress");
		modelBuilder.Entity<Progress>().Property(e => e.Type).HasConversion<string>();
		modelBuilder.Entity<Progress>().Property(e => e.Status).HasConversion<string>();
		modelBuilder.Entity<Progress>().HasIndex(e => e.LastUpdated);

		modelBuilder.Entity<CalculatedPoint>().ToTable("CalculatedPoints");

		modelBuilder.Entity<Command>().ToTable("Commands");
		modelBuilder.Entity<Command>().HasIndex(e => e.RuleId);
		modelBuilder.Entity<Command>().HasIndex(e => e.RuleInstanceId);
		modelBuilder.Entity<Command>().HasIndex(e => e.EquipmentId);
		modelBuilder.Entity<Command>().HasIndex(e => e.TwinId);

		modelBuilder.Entity<ADTSummary>().ToTable("ADTSummaries");

		modelBuilder.Entity<TimeSeries>().ToTable("TimeSeries");
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.UnitOfMeasure);
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.MaxValue);
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.MinValue);
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.AverageValue);
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.EstimatedPeriod);
		modelBuilder.Entity<TimeSeries>().HasIndex(e => e.TotalValuesProcessed);

		modelBuilder.Entity<TimeSeriesMapping>().ToTable("TimeSeriesMapping");
		modelBuilder.Entity<TimeSeriesMapping>().HasIndex(e => e.ConnectorId);
		modelBuilder.Entity<TimeSeriesMapping>().HasIndex(e => e.ExternalId);
		modelBuilder.Entity<TimeSeriesMapping>().HasIndex(e => e.TrendId);

		modelBuilder.Entity<ActorState>().ToTable("Actors");
		modelBuilder.Entity<ActorState>().HasIndex(e => e.RuleId);

		modelBuilder.Entity<RuleExecutionRequest>().ToTable("RuleExecutionRequest");
		modelBuilder.Entity<RuleExecutionRequest>().Property(e => e.Command).HasConversion<string>();

		modelBuilder.Entity<InsightChange>().ToTable("InsightChanges");
		modelBuilder.Entity<InsightChange>().HasIndex(e => e.InsightId);

		modelBuilder.Entity<LogEntry>().ToTable("Logs");
		modelBuilder.Entity<LogEntry>().HasNoKey();
		modelBuilder.Entity<LogEntry>().HasIndex(e => e.ProgressId);
		modelBuilder.Entity<LogEntry>().HasIndex(e => e.TimeStamp);
		modelBuilder.Entity<LogEntry>().Ignore(e => e.Id);

		modelBuilder.Entity<RuleTimeSeriesMapping>().ToTable("RuleTimeSeriesMapping");
		modelBuilder.Entity<RuleTimeSeriesMapping>().HasIndex(e => e.RuleId);

		modelBuilder.Entity<MLModel>().ToTable("MLModels");

		// Map fields to JSON
		modelBuilder.ApplyConfiguration(new RuleConfiguration());
		modelBuilder.ApplyConfiguration(new RuleInstanceConfiguration());
		modelBuilder.ApplyConfiguration(new InsightConfiguration());
		// modelBuilder.ApplyConfiguration(new CalculatedPointInstanceConfiguration());
		modelBuilder.ApplyConfiguration(new ProgressConfiguration());
		modelBuilder.ApplyConfiguration(new TimeSeriesBufferConfiguration());
		modelBuilder.ApplyConfiguration(new ActorStateConfiguration());
		modelBuilder.ApplyConfiguration(new RuleExecutionRequestConfiguration());
        modelBuilder.ApplyConfiguration(new GlobalVariableConfiguration());
		modelBuilder.ApplyConfiguration(new CommandConfiguration());
		modelBuilder.ApplyConfiguration(new ADTSummaryConfiguration());
		modelBuilder.ApplyConfiguration(new RuleMetadataConfiguration());
		modelBuilder.ApplyConfiguration(new MLModelConfiguration());
		modelBuilder.ApplyConfiguration(new RuleInstanceMetadataConfiguration());
		modelBuilder.ApplyConfiguration(new CalculatedPointConfiguration());
		modelBuilder.ApplyConfiguration(new TimeSeriesMappingConfiguration());
	}
}
