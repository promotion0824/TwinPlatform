﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations.Jobs
{
    [DbContext(typeof(JobsContext))]
    [Migration("20240508160113_JobsProgressFieldUpdate")]
    partial class JobsProgressFieldUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi.JobsEntry", b =>
                {
                    b.Property<string>("JobId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("CustomData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ErrorsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InputsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsExternal")
                        .HasColumnType("bit");

                    b.Property<string>("JobSubtype")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("JobType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("OutputsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ParentJobId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<DateTimeOffset?>("ProcessingEndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("ProcessingStartTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("ProgressCurrentCount")
                        .HasColumnType("int");

                    b.Property<string>("ProgressStatusMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ProgressTotalCount")
                        .HasColumnType("int");

                    b.Property<string>("SourceResourceUri")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("TargetResourceUri")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<DateTimeOffset>("TimeCreated")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("TimeLastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("UserMessage")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("JobId");

                    b.HasIndex("JobType", "Status", "IsDeleted");

                    b.ToTable("JobEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
