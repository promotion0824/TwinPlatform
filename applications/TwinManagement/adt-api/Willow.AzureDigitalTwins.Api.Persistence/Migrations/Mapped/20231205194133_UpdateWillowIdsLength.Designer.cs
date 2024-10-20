﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    [DbContext(typeof(MappingContext))]
    [Migration("20231205194133_UpdateWillowIdsLength")]
    partial class UpdateWillowIdsLength
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Willow.AzureDigitalTwins.Api.Persistence.MappedEntry", b =>
                {
                    b.Property<string>("MappedId")
                        .HasMaxLength(48)
                        .HasColumnType("nvarchar(48)");

                    b.Property<string>("AuditInformation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConnectorId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MappedModelId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ModelInformation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ParentMappedId")
                        .HasMaxLength(48)
                        .HasColumnType("nvarchar(48)");

                    b.Property<string>("ParentWillowId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("StatusNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("TimeCreated")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("TimeLastUpdated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("WillowId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("WillowModelId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("WillowParentRel")
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.HasKey("MappedId");

                    b.ToTable("MappedEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
