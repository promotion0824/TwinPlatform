﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Willow.Infrastructure.Entities;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    [DbContext(typeof(WsupDbContext))]
    [Migration("20240729163422_ConnectorsRedo")]
    partial class ConnectorsRedo
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Willow.Infrastructure.Entities.Application", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ApplicationStatusId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasPublicEndpoint")
                        .HasColumnType("bit");

                    b.Property<string>("HealthEndpointPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationStatusId");

                    b.HasIndex("TeamId");

                    b.ToTable("Applications", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ApplicationStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("ApplicationStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Application is not active.",
                            Name = "Inactive"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Application is active.",
                            Name = "Active"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Application is in Preview mode.",
                            Name = "Preview"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Building", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("CustomerInstanceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerInstanceId");

                    b.ToTable("Buildings", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.BuildingConnector", b =>
                {
                    b.Property<string>("BuildingId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConnectorId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("BuildingConnectorStatusId")
                        .HasColumnType("int");

                    b.HasKey("BuildingId", "ConnectorId");

                    b.HasIndex("BuildingConnectorStatusId");

                    b.HasIndex("ConnectorId");

                    b.ToTable("BuildingConnectors", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.BuildingConnectorStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("BuildingConnectorStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Connector is in the process of being deployed to this building.",
                            Name = "Commissioning"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Connector is now disabled for this building.",
                            Name = "Disabled"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Connector is temporarily offline for this building.",
                            Name = "Offline"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Building Connector is active.",
                            Name = "Active"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Connector", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("ConnectorStatusId")
                        .HasColumnType("int");

                    b.Property<string>("ConnectorTypeId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("CustomerInstanceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectorStatusId");

                    b.HasIndex("ConnectorTypeId");

                    b.HasIndex("CustomerInstanceId");

                    b.ToTable("Connectors", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ConnectorStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("ConnectorStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Connector is in development.",
                            Name = "In Development"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Connector is no longer active.",
                            Name = "Inactive"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Connector is active.",
                            Name = "Active"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ConnectorType", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Direction")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Version")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ConnectorTypes", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CustomerStatusId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("SalesId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerStatusId");

                    b.ToTable("Customers", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstance", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AzureDataExplorerInstance")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AzureDigitalTwinsInstance")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CustomerInstanceStatusId")
                        .HasColumnType("int");

                    b.Property<string>("DeploymentPhase")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DnsEnvSuffix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Domain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullCustomerInstanceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullDomain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LifecycleState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("RegionShortName")
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ResourceGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StampId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("CustomerInstanceStatusId");

                    b.HasIndex("RegionShortName");

                    b.HasIndex("StampId");

                    b.ToTable("CustomerInstances", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceApplication", b =>
                {
                    b.Property<Guid>("CustomerInstanceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ApplicationId")
                        .HasColumnType("int");

                    b.Property<int>("CustomerInstanceApplicationStatusId")
                        .HasColumnType("int");

                    b.HasKey("CustomerInstanceId", "ApplicationId");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("CustomerInstanceApplicationStatusId");

                    b.ToTable("CustomerInstanceApplications", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceApplicationStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("CustomerInstanceApplicationStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Application is not active.",
                            Name = "Inactive"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Application is active.",
                            Name = "Active"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdtInstanceUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AdxDatabaseUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ResourceGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CustomerInstanceStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Customer instance is not yet active.",
                            Name = "Commissioning"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Customer instance is active.",
                            Name = "Active"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Customer instance is no longer active.",
                            Name = "Decommissioned"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("CustomerStatuses", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Customer is not active.",
                            Name = "Inactive"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Customer is active.",
                            Name = "Active"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Environment", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Name");

                    b.ToTable("Environments", "wsup");

                    b.HasData(
                        new
                        {
                            Name = "dev",
                            Description = "Development and Test Environment."
                        },
                        new
                        {
                            Name = "prd",
                            Description = "Production Environment."
                        },
                        new
                        {
                            Name = "sbx",
                            Description = "Sandbox environment for developer testing."
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Region", b =>
                {
                    b.Property<string>("ShortName")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("ShortName");

                    b.ToTable("Regions", "wsup");

                    b.HasData(
                        new
                        {
                            ShortName = "aue",
                            DisplayName = "Australia East",
                            Name = "australiaeast"
                        },
                        new
                        {
                            ShortName = "eus",
                            DisplayName = "US East",
                            Name = "eastus"
                        },
                        new
                        {
                            ShortName = "eus2",
                            DisplayName = "US East 2",
                            Name = "eastus2"
                        },
                        new
                        {
                            ShortName = "weu",
                            DisplayName = "West Europe",
                            Name = "westeurope"
                        },
                        new
                        {
                            ShortName = "wus",
                            DisplayName = "West US",
                            Name = "westus"
                        },
                        new
                        {
                            ShortName = "wus2",
                            DisplayName = "West US 2",
                            Name = "westus2"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Stamp", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EnvironmentName")
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("RegionShortName")
                        .HasColumnType("nvarchar(10)");

                    b.Property<Guid>("SubscriptionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RegionShortName");

                    b.HasIndex("EnvironmentName", "RegionShortName", "Name")
                        .IsUnique()
                        .HasFilter("[EnvironmentName] IS NOT NULL AND [RegionShortName] IS NOT NULL AND [Name] IS NOT NULL");

                    b.ToTable("Stamps", "wsup");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Teams", "wsup");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Cloud Operations",
                            Name = "CloudOps"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Activate Technology Team",
                            Name = "Activate Technology"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Advanced Analytics",
                            Name = "Advanced Analytics"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Connectors",
                            Name = "Connectors"
                        },
                        new
                        {
                            Id = 5,
                            Description = "Core Services",
                            Name = "CoreServices"
                        },
                        new
                        {
                            Id = 6,
                            Description = "Dashboards",
                            Name = "Dashboards"
                        },
                        new
                        {
                            Id = 7,
                            Description = "Investa Experience",
                            Name = "InvestaExperience"
                        },
                        new
                        {
                            Id = 8,
                            Description = "IoT Services",
                            Name = "IoTServices"
                        },
                        new
                        {
                            Id = 9,
                            Description = "Search and Explore",
                            Name = "SearchAndExplore"
                        },
                        new
                        {
                            Id = 10,
                            Description = "Security and Privacy",
                            Name = "Security"
                        },
                        new
                        {
                            Id = 11,
                            Description = "Workflows",
                            Name = "Workflows"
                        },
                        new
                        {
                            Id = 999999,
                            Description = "Team is Unknown.",
                            Name = "Unknown"
                        });
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Application", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.ApplicationStatus", "ApplicationStatus")
                        .WithMany("Applications")
                        .HasForeignKey("ApplicationStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Application_ApplicationStatuses");

                    b.HasOne("Willow.Infrastructure.Entities.Team", "Team")
                        .WithMany("Applications")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Application_Team");

                    b.Navigation("ApplicationStatus");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Building", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.CustomerInstance", "CustomerInstance")
                        .WithMany("Buildings")
                        .HasForeignKey("CustomerInstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Building_CustomerInstance");

                    b.Navigation("CustomerInstance");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.BuildingConnector", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.BuildingConnectorStatus", "BuildingConnectorStatus")
                        .WithMany("BuildingConnectors")
                        .HasForeignKey("BuildingConnectorStatusId")
                        .IsRequired()
                        .HasConstraintName("FK_BuildingConnector_BuildingConnectorStatuses");

                    b.HasOne("Willow.Infrastructure.Entities.Building", "Building")
                        .WithMany("BuildingConnectors")
                        .HasForeignKey("BuildingId")
                        .IsRequired()
                        .HasConstraintName("FK_BuildingConnector_Building");

                    b.HasOne("Willow.Infrastructure.Entities.Connector", "Connector")
                        .WithMany("BuildingConnectors")
                        .HasForeignKey("ConnectorId")
                        .IsRequired()
                        .HasConstraintName("FK_BuildingConnector_Connector");

                    b.Navigation("Building");

                    b.Navigation("BuildingConnectorStatus");

                    b.Navigation("Connector");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Connector", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.ConnectorStatus", "ConnectorStatus")
                        .WithMany("Connectors")
                        .HasForeignKey("ConnectorStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Connector_ConnectorStatuses");

                    b.HasOne("Willow.Infrastructure.Entities.ConnectorType", "ConnectorType")
                        .WithMany("Connectors")
                        .HasForeignKey("ConnectorTypeId")
                        .HasConstraintName("FK_Connector_ConnectorTypes");

                    b.HasOne("Willow.Infrastructure.Entities.CustomerInstance", "CustomerInstance")
                        .WithMany("Connectors")
                        .HasForeignKey("CustomerInstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Connector_CustomerInstance");

                    b.Navigation("ConnectorStatus");

                    b.Navigation("ConnectorType");

                    b.Navigation("CustomerInstance");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Customer", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.CustomerStatus", "CustomerStatus")
                        .WithMany("Customers")
                        .HasForeignKey("CustomerStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Customer_CustomerStatuses");

                    b.Navigation("CustomerStatus");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstance", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.Customer", "Customer")
                        .WithMany("CustomerInstances")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstance_Customer");

                    b.HasOne("Willow.Infrastructure.Entities.CustomerInstanceStatus", "CustomerInstanceStatus")
                        .WithMany("CustomerInstances")
                        .HasForeignKey("CustomerInstanceStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstance_CustomerInstanceStatuses");

                    b.HasOne("Willow.Infrastructure.Entities.Region", null)
                        .WithMany("CustomerInstances")
                        .HasForeignKey("RegionShortName");

                    b.HasOne("Willow.Infrastructure.Entities.Stamp", "Stamp")
                        .WithMany("CustomerInstances")
                        .HasForeignKey("StampId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstance_Stamp");

                    b.Navigation("Customer");

                    b.Navigation("CustomerInstanceStatus");

                    b.Navigation("Stamp");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceApplication", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.Application", "Application")
                        .WithMany("CustomerInstanceApplications")
                        .HasForeignKey("ApplicationId")
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstanceApplication_Applications");

                    b.HasOne("Willow.Infrastructure.Entities.CustomerInstanceApplicationStatus", "CustomerInstanceApplicationStatus")
                        .WithMany("CustomerInstanceApplications")
                        .HasForeignKey("CustomerInstanceApplicationStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstanceApplication_CustomerInstanceApplicationStatuses");

                    b.HasOne("Willow.Infrastructure.Entities.CustomerInstance", "CustomerInstance")
                        .WithMany("CustomerInstanceApplications")
                        .HasForeignKey("CustomerInstanceId")
                        .IsRequired()
                        .HasConstraintName("FK_CustomerInstanceApplication_CustomerInstances");

                    b.Navigation("Application");

                    b.Navigation("CustomerInstance");

                    b.Navigation("CustomerInstanceApplicationStatus");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Stamp", b =>
                {
                    b.HasOne("Willow.Infrastructure.Entities.Environment", "Environment")
                        .WithMany("Stamps")
                        .HasForeignKey("EnvironmentName");

                    b.HasOne("Willow.Infrastructure.Entities.Region", "Region")
                        .WithMany("Stamps")
                        .HasForeignKey("RegionShortName");

                    b.Navigation("Environment");

                    b.Navigation("Region");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Application", b =>
                {
                    b.Navigation("CustomerInstanceApplications");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ApplicationStatus", b =>
                {
                    b.Navigation("Applications");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Building", b =>
                {
                    b.Navigation("BuildingConnectors");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.BuildingConnectorStatus", b =>
                {
                    b.Navigation("BuildingConnectors");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Connector", b =>
                {
                    b.Navigation("BuildingConnectors");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ConnectorStatus", b =>
                {
                    b.Navigation("Connectors");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.ConnectorType", b =>
                {
                    b.Navigation("Connectors");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Customer", b =>
                {
                    b.Navigation("CustomerInstances");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstance", b =>
                {
                    b.Navigation("Buildings");

                    b.Navigation("Connectors");

                    b.Navigation("CustomerInstanceApplications");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceApplicationStatus", b =>
                {
                    b.Navigation("CustomerInstanceApplications");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerInstanceStatus", b =>
                {
                    b.Navigation("CustomerInstances");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.CustomerStatus", b =>
                {
                    b.Navigation("Customers");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Environment", b =>
                {
                    b.Navigation("Stamps");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Region", b =>
                {
                    b.Navigation("CustomerInstances");

                    b.Navigation("Stamps");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Stamp", b =>
                {
                    b.Navigation("CustomerInstances");
                });

            modelBuilder.Entity("Willow.Infrastructure.Entities.Team", b =>
                {
                    b.Navigation("Applications");
                });
#pragma warning restore 612, 618
        }
    }
}
