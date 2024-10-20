﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Willow.Rules.Repository;

#nullable disable

namespace WillowRules.Migrations
{
    [DbContext(typeof(RulesContext))]
    [Migration("20230607124016_NoneScriptFix")]
    partial class NoneScriptFix
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Willow.Rules.Model.ADTSummary", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ADTInstanceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("AsOfDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("CountCapabilities")
                        .HasColumnType("int");

                    b.Property<int>("CountModels")
                        .HasColumnType("int");

                    b.Property<int>("CountModelsInUse")
                        .HasColumnType("int");

                    b.Property<int>("CountRelationships")
                        .HasColumnType("int");

                    b.Property<int>("CountTwins")
                        .HasColumnType("int");

                    b.Property<int>("CountTwinsNotInGraph")
                        .HasColumnType("int");

                    b.Property<string>("CustomerEnvironmentId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ADTSummaries", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.ActorState", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("EarliestSeen")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Invocations")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsValid")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedOutput")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastFaulted")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("OutputValues")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RuleInstanceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("TimedValues")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("RuleId");

                    b.ToTable("Actors", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.CalculatedPoint", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrendId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ValueExpression")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CalculatedPoints", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.GlobalVariable", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Expression")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBuiltIn")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("GlobalVariable", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.ImpactScore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("BaseScore")
                        .HasColumnType("float");

                    b.Property<string>("FieldId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InsightId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("Score")
                        .HasColumnType("float");

                    b.Property<string>("Unit")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BaseScore");

                    b.HasIndex("InsightId");

                    b.HasIndex("LastUpdated");

                    b.HasIndex("Name");

                    b.HasIndex("Score");

                    b.ToTable("InsightImpactScore", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.Insight", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("CommandEnabled")
                        .HasColumnType("bit");

                    b.Property<Guid>("CommandInsightId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("EarliestFaultedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("EquipmentId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("EquipmentName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("EquipmentUniqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("FaultedCount")
                        .HasColumnType("int");

                    b.Property<string>("FedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Feeds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Invocations")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsFaulty")
                        .HasColumnType("bit");

                    b.Property<bool>("IsValid")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastFaultedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Locations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Occurrences")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PrimaryModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleCategory")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RuleInstanceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleRecomendations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleTemplateName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SiteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TimeZone")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EquipmentId");

                    b.HasIndex("EquipmentName");

                    b.HasIndex("LastFaultedDate");

                    b.HasIndex("LastUpdated");

                    b.HasIndex("RuleId");

                    b.ToTable("Insight", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.InsightChange", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("InsightId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("InsightId");

                    b.ToTable("InsightChanges", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.Progress", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CorrelationId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CurrentTimeSeriesTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("DateRequested")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("EndTimeSeriesTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("EntityId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Eta")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("FailedReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InnerProgress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("Percentage")
                        .HasColumnType("float");

                    b.Property<string>("RequestedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Speed")
                        .HasColumnType("float");

                    b.Property<DateTimeOffset>("StartTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("StartTimeSeriesTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LastUpdated");

                    b.ToTable("Progress", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.Rule", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Category")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("CommandEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Elements")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Filters")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImpactScores")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDraft")
                        .HasColumnType("bit");

                    b.Property<string>("LanguageDescriptions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LanguageNames")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LanguageRecommendations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Parameters")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimaryModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Recommendations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TemplateId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("RuleId");

                    b.ToTable("Rule", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.RuleExecution", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("CompletedEndDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("CustomerEnvironmentId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("Generation")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Percentage")
                        .HasColumnType("float");

                    b.Property<double>("PercentageReported")
                        .HasColumnType("float");

                    b.Property<string>("RuleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("TargetEndDate")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("RuleId");

                    b.ToTable("RuleExecution", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.RuleExecutionRequest", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerEnvironmentId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtendedData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProgressId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Requested")
                        .HasColumnType("bit");

                    b.Property<string>("RequestedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("RequestedDate")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("RuleExecutionRequest", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.RuleInstance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("CommandEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<string>("EquipmentId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("EquipmentName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("EquipmentUniqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Feeds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Locations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OutputTrendId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PointEntityIds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimaryModelId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RuleCategory")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleFiltersBound")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RuleImpactScoresBound")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleParametersBound")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SiteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("TimeZone")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EquipmentId");

                    b.HasIndex("EquipmentName");

                    b.HasIndex("LastUpdated");

                    b.HasIndex("PrimaryModelId");

                    b.HasIndex("RuleId");

                    b.HasIndex("Status");

                    b.ToTable("RuleInstance", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.RuleInstanceMetadata", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("LastTriggered")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("TriggerCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("RuleInstanceMetadata", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.RuleMetadata", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ETag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("EarliestExecutionDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("InsightsGenerated")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("LastModified")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RuleInstanceCount")
                        .HasColumnType("int");

                    b.Property<bool>("ScanComplete")
                        .HasColumnType("bit");

                    b.Property<string>("ScanError")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ScanStarted")
                        .HasColumnType("bit");

                    b.Property<string>("ScanState")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("ScanStateAsOf")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("ValidInstanceCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("RuleMetadata", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.TimeSeries", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AveragePeriodEstimator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("AverageValue")
                        .HasColumnType("float");

                    b.Property<string>("CompressionState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConnectorId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DtId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("EarliestSeen")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("EstimatedPeriod")
                        .HasColumnType("bigint");

                    b.Property<string>("ExternalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsNonMonotonicAscending")
                        .HasColumnType("bit");

                    b.Property<bool>("IsOffline")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPeriodOutOfRange")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStuck")
                        .HasColumnType("bit");

                    b.Property<bool>("IsValueOutOfRange")
                        .HasColumnType("bit");

                    b.Property<long>("LastGap")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastSeen")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool?>("LastValueBool")
                        .HasColumnType("bit");

                    b.Property<double?>("LastValueDouble")
                        .HasColumnType("float");

                    b.Property<string>("LastValueString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaxCountToKeep")
                        .HasColumnType("int");

                    b.Property<long?>("MaxTimeToKeep")
                        .HasColumnType("bigint");

                    b.Property<double>("MaxValue")
                        .HasColumnType("float");

                    b.Property<double>("MinValue")
                        .HasColumnType("float");

                    b.Property<string>("ModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MonotonicAscendingEstimator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Points")
                        .HasColumnType("varbinary(max)");

                    b.Property<double>("TotalValue")
                        .HasColumnType("float");

                    b.Property<long>("TotalValuesProcessed")
                        .HasColumnType("bigint");

                    b.Property<int?>("TrendInterval")
                        .HasColumnType("int");

                    b.Property<string>("UnitOfMeasure")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ValueOutOfRangeEstimator")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AverageValue");

                    b.HasIndex("EstimatedPeriod");

                    b.HasIndex("MaxValue");

                    b.HasIndex("MinValue");

                    b.HasIndex("TotalValuesProcessed");

                    b.HasIndex("UnitOfMeasure");

                    b.ToTable("TimeSeries", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.TimeSeriesMapping", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConnectorId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DtId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExternalId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("LastUpdate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrendId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("TrendInterval")
                        .HasColumnType("int");

                    b.Property<string>("Unit")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectorId");

                    b.HasIndex("ExternalId");

                    b.HasIndex("TrendId");

                    b.ToTable("TimeSeriesMapping", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.ImpactScore", b =>
                {
                    b.HasOne("Willow.Rules.Model.Insight", "Insight")
                        .WithMany("ImpactScores")
                        .HasForeignKey("InsightId");

                    b.Navigation("Insight");
                });

            modelBuilder.Entity("Willow.Rules.Model.Rule", b =>
                {
                    b.HasOne("Willow.Rules.Model.Rule", null)
                        .WithMany("ParentIds")
                        .HasForeignKey("RuleId");
                });

            modelBuilder.Entity("Willow.Rules.Model.Insight", b =>
                {
                    b.Navigation("ImpactScores");
                });

            modelBuilder.Entity("Willow.Rules.Model.Rule", b =>
                {
                    b.Navigation("ParentIds");
                });
#pragma warning restore 612, 618
        }
    }
}
