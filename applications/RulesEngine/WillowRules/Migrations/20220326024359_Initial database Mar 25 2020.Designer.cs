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
    [Migration("20220326024359_Initial database Mar 25 2020")]
    partial class InitialdatabaseMar252020
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

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

            modelBuilder.Entity("Willow.Rules.Model.CalculatedPoint", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

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

            modelBuilder.Entity("Willow.Rules.Model.Insight", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("Comfort")
                        .HasColumnType("float");

                    b.Property<bool>("CommandEnabled")
                        .HasColumnType("bit");

                    b.Property<Guid>("CommandInsightId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Cost")
                        .HasColumnType("float");

                    b.Property<string>("EquipmentId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("EquipmentUniqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Feeds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Invocations")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Locations")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Occurrences")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimaryModelId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Reliability")
                        .HasColumnType("float");

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

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Comfort");

                    b.HasIndex("Cost");

                    b.HasIndex("EquipmentId");

                    b.HasIndex("LastUpdated");

                    b.HasIndex("Reliability");

                    b.HasIndex("RuleId");

                    b.ToTable("Insight", (string)null);
                });

            modelBuilder.Entity("Willow.Rules.Model.Progress", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CorrelationId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CurrentTimeSeriesTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("EndTimeSeriesTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("EntityId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Eta")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("Percentage")
                        .HasColumnType("float");

                    b.Property<long>("RelationshipCount")
                        .HasColumnType("bigint");

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

                    b.Property<int>("TotalRelationshipCount")
                        .HasColumnType("int");

                    b.Property<int>("TotalTwinCount")
                        .HasColumnType("int");

                    b.Property<long>("TwinCount")
                        .HasColumnType("bigint");

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

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Elements")
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

            modelBuilder.Entity("Willow.Rules.Model.RuleInstance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<string>("EquipmentId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("EquipmentUniqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Feeds")
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<string>("RuleId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleParametersBound")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RuleTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SiteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Valid")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("EquipmentId");

                    b.HasIndex("PrimaryModelId");

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

                    b.Property<string>("ETag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("InsightsGenerated")
                        .HasColumnType("int");

                    b.Property<int>("RuleInstanceCount")
                        .HasColumnType("int");

                    b.Property<bool>("ScanComplete")
                        .HasColumnType("bit");

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

            modelBuilder.Entity("Willow.Rules.Model.Rule", b =>
                {
                    b.HasOne("Willow.Rules.Model.Rule", null)
                        .WithMany("ParentIds")
                        .HasForeignKey("RuleId");
                });

            modelBuilder.Entity("Willow.Rules.Model.Rule", b =>
                {
                    b.Navigation("ParentIds");
                });
#pragma warning restore 612, 618
        }
    }
}
