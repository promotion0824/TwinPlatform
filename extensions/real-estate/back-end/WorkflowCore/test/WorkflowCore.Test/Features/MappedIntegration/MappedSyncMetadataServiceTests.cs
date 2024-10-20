using AutoFixture;
using FluentAssertions;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Interfaces;
using WorkflowCore.Services.MappedIntegration.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.MappedIntegration;

public class MappedSyncMetadataServiceTests : BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public MappedSyncMetadataServiceTests(ITestOutputHelper output) : base(output)
    {
        var inMemorySettings = new Dictionary<string, string>
            {
                { "MappedIntegrationConfiguration:IsEnabled", "true" },
                { "MappedIntegrationConfiguration:IsTicketMetaDataSyncEnabled","true"}
            };
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task SyncTicketMetadata_MappedSyncMetadataService_Success()
    {
        var ticketMetaDataResponse = Fixture.Build<TicketMetadataResponse>()
                                            .Create();
        var spaceTwinDict = new Dictionary<string, string>();
        var dtResponse = new List<BuildingsTwinDto>();

        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            // build the spaceTwinDict to be used in the expected digitTwin response
            spaceTwinDict.Add(item.SpaceId.ToString("N"), $"twin-{item.SpaceId}");
        }

        // expected results
        var expectedServiceNeeded = new List<ServiceNeededEntity>();
        foreach (var item in ticketMetaDataResponse.ServiceNeededList)
        {
            expectedServiceNeeded.Add(new ServiceNeededEntity
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.RequestTypeId
            });
        }

        // set up the  response of space service needed list
        // the space id and service needed mush match the digit twin response and service needed list ids
        var spaceSpaceServiceNeededList = new List<SpaceServiceNeeded>();
        foreach (var item in spaceTwinDict)
        {
            var serviceNeededIdsList = ticketMetaDataResponse.ServiceNeededList.Select(x => x.Id).ToList();
            spaceSpaceServiceNeededList.Add(new SpaceServiceNeeded(new Guid(), Guid.Parse(item.Key), serviceNeededIdsList));

        }
        ticketMetaDataResponse.SpaceServiceNeededList = spaceSpaceServiceNeededList;

        // set up the expected results of service needed space twin
        var expectedServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                expectedServiceNeededSpaceTwin.Add(new ServiceNeededSpaceTwinEntity
                {
                    ServiceNeededId = serviceNeededId,
                    SpaceTwinId = spaceTwinDict.GetValueOrDefault(item.SpaceId.ToString("N"))
                });
            }

        }

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var logger = new Mock<ILogger<MappedSyncMetadataService>>();
            var httpFactory = new Mock<IHttpClientFactory>();


            var mappedApiServiceMock = new Mock<IMappedApiService>();
            var dtApiMock = new Mock<IDigitalTwinServiceApi>();
            var cacheMock = new Mock<IAppCache>();

            mappedApiServiceMock.Setup(x => x.GetTicketMetaDataAsync(It.IsAny<MappedIntegrationConfiguration>()))
                                .ReturnsAsync(ticketMetaDataResponse);
            cacheMock.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<Dictionary<string, string>>>>()))
                     .ReturnsAsync(spaceTwinDict);

            var service = new MappedSyncMetadataService(httpFactory.Object,
                                                        logger.Object,
                                                        db,
                                                        configuration,
                                                        mappedApiServiceMock.Object,
                                                        dtApiMock.Object,
                                                        cacheMock.Object);

            await service.SyncTicketMetadata();
            var jobTypes = db.JobTypes.ToList();
            var categories = db.TicketCategories.ToList();
            var serviceNeeded = db.ServiceNeeded.ToList();
            var serviceNeededSpaceTwin = db.ServiceNeededSpaceTwin.ToList();
            // assert

            jobTypes.Should().BeEquivalentTo(ticketMetaDataResponse.JobTypes);
            categories.Should().BeEquivalentTo(ticketMetaDataResponse.RequestTypes);
            serviceNeeded.Should().BeEquivalentTo(expectedServiceNeeded, config => config.Excluding(x => x.LastUpdate));
            serviceNeededSpaceTwin.Should().BeEquivalentTo(expectedServiceNeededSpaceTwin, config =>
            {
                config.Excluding(x => x.LastUpdate);
                config.Excluding(x => x.Id);
                return config;
            });
        }

    }

    [Fact]
    public async Task SyncTicketMetadataWithSameExitingData_MappedSyncMetadataService_SuccessWithNoChange()
    {
        var ticketMetaDataResponse = Fixture.Build<TicketMetadataResponse>()
                                            .Create();
        var spaceTwinDict = new Dictionary<string, string>();
        var dtResponse = new List<BuildingsTwinDto>();

        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            // build the spaceTwinDict to be used in the expected digitTwin response
            spaceTwinDict.Add(item.SpaceId.ToString("N"), $"twin-{item.SpaceId}");
        }

        // expected results
        var expectedServiceNeeded = new List<ServiceNeededEntity>();
        foreach (var item in ticketMetaDataResponse.ServiceNeededList)
        {
            expectedServiceNeeded.Add(new ServiceNeededEntity
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.RequestTypeId
            });
        }

        // set up the  response of space service needed list
        // the space id and service needed mush match the digit twin response and service needed list ids
        var spaceSpaceServiceNeededList = new List<SpaceServiceNeeded>();
        foreach (var item in spaceTwinDict)
        {
            var serviceNeededIdsList = ticketMetaDataResponse.ServiceNeededList.Select(x => x.Id).ToList();
            spaceSpaceServiceNeededList.Add(new SpaceServiceNeeded(new Guid(), Guid.Parse(item.Key), serviceNeededIdsList));

        }
        ticketMetaDataResponse.SpaceServiceNeededList = spaceSpaceServiceNeededList;

        // set up the expected results of service needed space twin
        var expectedServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                expectedServiceNeededSpaceTwin.Add(new ServiceNeededSpaceTwinEntity
                {
                    ServiceNeededId = serviceNeededId,
                    SpaceTwinId = spaceTwinDict.GetValueOrDefault(item.SpaceId.ToString("N"))
                });
            }

        }

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var logger = new Mock<ILogger<MappedSyncMetadataService>>();
            var httpFactory = new Mock<IHttpClientFactory>();


            var mappedApiServiceMock = new Mock<IMappedApiService>();
            var dtApiMock = new Mock<IDigitalTwinServiceApi>();
            var cacheMock = new Mock<IAppCache>();

            mappedApiServiceMock.Setup(x => x.GetTicketMetaDataAsync(It.IsAny<MappedIntegrationConfiguration>()))
                                .ReturnsAsync(ticketMetaDataResponse);
            cacheMock.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<Dictionary<string, string>>>>()))
                     .ReturnsAsync(spaceTwinDict);

            var service = new MappedSyncMetadataService(httpFactory.Object,
                                                        logger.Object,
                                                        db,
                                                        configuration,
                                                        mappedApiServiceMock.Object,
                                                        dtApiMock.Object,
                                                        cacheMock.Object);

            // existing data in database
            var existingJobTypes = ticketMetaDataResponse.JobTypes.Select(x => new JobTypeEntity
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            var existingCategories = ticketMetaDataResponse.RequestTypes.Select(x => new TicketCategoryEntity
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            var existingServiceNeeded = ticketMetaDataResponse.ServiceNeededList.Select(x => new ServiceNeededEntity
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.RequestTypeId
            }).ToList();

            db.JobTypes.AddRange(existingJobTypes);
            db.TicketCategories.AddRange(existingCategories);
            db.ServiceNeeded.AddRange(existingServiceNeeded);
            db.ServiceNeededSpaceTwin.AddRange(expectedServiceNeededSpaceTwin);
            db.SaveChanges();
            await service.SyncTicketMetadata();
            var jobTypes = db.JobTypes.ToList();
            var categories = db.TicketCategories.ToList();
            var serviceNeeded = db.ServiceNeeded.ToList();
            var serviceNeededSpaceTwin = db.ServiceNeededSpaceTwin.ToList();

            // assert
            jobTypes.Should().BeEquivalentTo(ticketMetaDataResponse.JobTypes);
            categories.Should().BeEquivalentTo(ticketMetaDataResponse.RequestTypes);
            serviceNeeded.Should().BeEquivalentTo(expectedServiceNeeded, config => config.Excluding(x => x.LastUpdate));
            serviceNeededSpaceTwin.Should().BeEquivalentTo(expectedServiceNeededSpaceTwin, config =>
            {
                config.Excluding(x => x.LastUpdate);
                config.Excluding(x => x.ServiceNeeded);
                config.Excluding(x => x.Id);
                return config;
            });
        }

    }

    [Fact]
    public async Task SyncTicketMetadataWithPartialExitingData_MappedSyncMetadataService_SuccessWithNewChanges()
    {
        var ticketMetaDataResponse = Fixture.Build<TicketMetadataResponse>()
                                            .Create();
        var spaceTwinDict = new Dictionary<string, string>();
        var dtResponse = new List<BuildingsTwinDto>();

        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            // build the spaceTwinDict to be used in the expected digitTwin response
            spaceTwinDict.Add(item.SpaceId.ToString("N"), $"twin-{item.SpaceId}");
        }

        // expected results
        var expectedServiceNeeded = new List<ServiceNeededEntity>();
        foreach (var item in ticketMetaDataResponse.ServiceNeededList)
        {
            expectedServiceNeeded.Add(new ServiceNeededEntity
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.RequestTypeId
            });
        }

        // set up the  response of space service needed list
        // the space id and service needed mush match the digit twin response and service needed list ids
        var spaceSpaceServiceNeededList = new List<SpaceServiceNeeded>();
        foreach (var item in spaceTwinDict)
        {
            var serviceNeededIdsList = ticketMetaDataResponse.ServiceNeededList.Select(x => x.Id).ToList();
            spaceSpaceServiceNeededList.Add(new SpaceServiceNeeded(new Guid(), Guid.Parse(item.Key), serviceNeededIdsList));

        }
        ticketMetaDataResponse.SpaceServiceNeededList = spaceSpaceServiceNeededList;

        // set up the expected results of service needed space twin
        var expectedServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                expectedServiceNeededSpaceTwin.Add(new ServiceNeededSpaceTwinEntity
                {
                    ServiceNeededId = serviceNeededId,
                    SpaceTwinId = spaceTwinDict.GetValueOrDefault(item.SpaceId.ToString("N"))
                });
            }

        }
        // remove the first space service needed from the expected results
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var logger = new Mock<ILogger<MappedSyncMetadataService>>();
            var httpFactory = new Mock<IHttpClientFactory>();


            var mappedApiServiceMock = new Mock<IMappedApiService>();
            var dtApiMock = new Mock<IDigitalTwinServiceApi>();
            var cacheMock = new Mock<IAppCache>();

            mappedApiServiceMock.Setup(x => x.GetTicketMetaDataAsync(It.IsAny<MappedIntegrationConfiguration>()))
                                .ReturnsAsync(ticketMetaDataResponse);
            cacheMock.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<Dictionary<string, string>>>>()))
                     .ReturnsAsync(spaceTwinDict);

            var service = new MappedSyncMetadataService(httpFactory.Object,
                                                        logger.Object,
                                                        db,
                                                        configuration,
                                                        mappedApiServiceMock.Object,
                                                        dtApiMock.Object,
                                                        cacheMock.Object);

            // existing data in database
            var existingJobTypes = ticketMetaDataResponse.JobTypes.Select(x => new JobTypeEntity
            {
                Id = x.Id,
                Name = x.Name
            }).FirstOrDefault();
            // extra job type that should be marked as inActive after sync
            var extraJobType = new JobTypeEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Job Type",
                IsActive = true
            };
            var existingCategories = ticketMetaDataResponse.RequestTypes.Select(x => new TicketCategoryEntity
            {
                Id = x.Id,
                Name = x.Name
            }).FirstOrDefault();
            // extra category that should be marked as inActive after sync
            var extraCategory = new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Category",
                IsActive = true
            };
            var existingServiceNeeded = ticketMetaDataResponse.ServiceNeededList.Select(x => new ServiceNeededEntity
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.RequestTypeId
            }).FirstOrDefault();
            // extra service needed that should be marked as inActive after sync
            var extraServiceNeeded = new ServiceNeededEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Service Needed",
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };
            var existingServiceNeededSpaceTwin = expectedServiceNeededSpaceTwin.FirstOrDefault();
            // extra service needed space twin that should be marked as deleted after sync
            var extraServiceNeededSpaceTwin = new ServiceNeededSpaceTwinEntity
            {
                Id = Guid.NewGuid(),
                ServiceNeededId = Guid.NewGuid(),
                SpaceTwinId = expectedServiceNeededSpaceTwin.Select(x => x.SpaceTwinId).FirstOrDefault()
            };
            db.JobTypes.Add(existingJobTypes);
            db.JobTypes.Add(extraJobType);
            db.TicketCategories.Add(existingCategories);
            db.TicketCategories.Add(extraCategory);
            db.ServiceNeeded.Add(existingServiceNeeded);
            db.ServiceNeeded.Add(extraServiceNeeded);
            db.ServiceNeededSpaceTwin.Add(existingServiceNeededSpaceTwin);
            db.ServiceNeededSpaceTwin.Add(extraServiceNeededSpaceTwin);
            db.SaveChanges();
            await service.SyncTicketMetadata();
            var jobTypes = db.JobTypes.ToList();
            var categories = db.TicketCategories.ToList();
            var serviceNeeded = db.ServiceNeeded.ToList();
            var serviceNeededSpaceTwin = db.ServiceNeededSpaceTwin.ToList();

            // assert
            jobTypes.Should().HaveCount(4);
            categories.Should().HaveCount(4);
            serviceNeeded.Should().HaveCount(4);
            serviceNeededSpaceTwin.Should().HaveCount(9);

            jobTypes.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.JobTypes);
            categories.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.RequestTypes);
            serviceNeeded.Where(x => x.IsActive).Should().BeEquivalentTo(expectedServiceNeeded, config => config.Excluding(x => x.LastUpdate));
            serviceNeededSpaceTwin.Should().BeEquivalentTo(expectedServiceNeededSpaceTwin, config =>
            {
                config.Excluding(x => x.LastUpdate);
                config.Excluding(x => x.ServiceNeeded);
                config.Excluding(x => x.Id);
                return config;
            });

            // extra data should be marked as inActive
            jobTypes.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraJobType);
            categories.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraCategory);
            serviceNeeded.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraServiceNeeded);
        }

    }

    [Fact]
    public async Task SyncTicketMetadataUpdateNames_MappedSyncMetadataService_SuccessWithNewChanges()
    {
        var ticketMetaDataResponse = Fixture.Build<TicketMetadataResponse>()
                                            .Create();
        var spaceTwinDict = new Dictionary<string, string>();
        var dtResponse = new List<BuildingsTwinDto>();

        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            // build the spaceTwinDict to be used in the expected digitTwin response
            spaceTwinDict.Add(item.SpaceId.ToString("N"), $"twin-{item.SpaceId}");
        }

        // expected results
        var expectedServiceNeeded = new List<ServiceNeededEntity>();
        foreach (var item in ticketMetaDataResponse.ServiceNeededList)
        {
            expectedServiceNeeded.Add(new ServiceNeededEntity
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.RequestTypeId
            });
        }

        // set up the  response of space service needed list
        // the space id and service needed mush match the digit twin response and service needed list ids
        var spaceSpaceServiceNeededList = new List<SpaceServiceNeeded>();
        foreach (var item in spaceTwinDict)
        {
            var serviceNeededIdsList = ticketMetaDataResponse.ServiceNeededList.Select(x => x.Id).ToList();
            spaceSpaceServiceNeededList.Add(new SpaceServiceNeeded(new Guid(), Guid.Parse(item.Key), serviceNeededIdsList));

        }
        ticketMetaDataResponse.SpaceServiceNeededList = spaceSpaceServiceNeededList;

        // set up the expected results of service needed space twin
        var expectedServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                expectedServiceNeededSpaceTwin.Add(new ServiceNeededSpaceTwinEntity
                {
                    ServiceNeededId = serviceNeededId,
                    SpaceTwinId = spaceTwinDict.GetValueOrDefault(item.SpaceId.ToString("N"))
                });
            }

        }
        // remove the first space service needed from the expected results
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var logger = new Mock<ILogger<MappedSyncMetadataService>>();
            var httpFactory = new Mock<IHttpClientFactory>();


            var mappedApiServiceMock = new Mock<IMappedApiService>();
            var dtApiMock = new Mock<IDigitalTwinServiceApi>();
            var cacheMock = new Mock<IAppCache>();

            mappedApiServiceMock.Setup(x => x.GetTicketMetaDataAsync(It.IsAny<MappedIntegrationConfiguration>()))
                                .ReturnsAsync(ticketMetaDataResponse);
            cacheMock.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<Dictionary<string, string>>>>()))
                     .ReturnsAsync(spaceTwinDict);

            var service = new MappedSyncMetadataService(httpFactory.Object,
                                                        logger.Object,
                                                        db,
                                                        configuration,
                                                        mappedApiServiceMock.Object,
                                                        dtApiMock.Object,
                                                        cacheMock.Object);

            // existing data in database
            var existingJobTypes = ticketMetaDataResponse.JobTypes.Select(x => new JobTypeEntity
            {
                Id = x.Id,
                Name = "different jobtype name",
                IsActive = false
            }).FirstOrDefault();
            // extra job type that should be marked as inActive after sync
            var extraJobType = new JobTypeEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Job Type",
                IsActive = true
            };
            var existingCategories = ticketMetaDataResponse.RequestTypes.Select(x => new TicketCategoryEntity
            {
                Id = x.Id,
                Name = "different category name",
                IsActive = false
            }).FirstOrDefault();
            // extra category that should be marked as inActive after sync
            var extraCategory = new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Category",
                IsActive = true
            };
            var existingServiceNeeded = ticketMetaDataResponse.ServiceNeededList.Select(x => new ServiceNeededEntity
            {
                Id = x.Id,
                Name = "different service needed name",
                CategoryId = Guid.NewGuid(),
                IsActive = false
            }).FirstOrDefault();
            // extra service needed that should be marked as inActive after sync
            var extraServiceNeeded = new ServiceNeededEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Service Needed",
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };
            var existingServiceNeededSpaceTwin = expectedServiceNeededSpaceTwin.FirstOrDefault();
            // extra service needed space twin that should be marked as deleted after sync
            var extraServiceNeededSpaceTwin = new ServiceNeededSpaceTwinEntity
            {
                Id = Guid.NewGuid(),
                ServiceNeededId = Guid.NewGuid(),
                SpaceTwinId = expectedServiceNeededSpaceTwin.Select(x => x.SpaceTwinId).FirstOrDefault()
            };
            db.JobTypes.Add(existingJobTypes);
            db.JobTypes.Add(extraJobType);
            db.TicketCategories.Add(existingCategories);
            db.TicketCategories.Add(extraCategory);
            db.ServiceNeeded.Add(existingServiceNeeded);
            db.ServiceNeeded.Add(extraServiceNeeded);
            db.ServiceNeededSpaceTwin.Add(existingServiceNeededSpaceTwin);
            db.ServiceNeededSpaceTwin.Add(extraServiceNeededSpaceTwin);
            db.SaveChanges();
            await service.SyncTicketMetadata();
            var jobTypes = db.JobTypes.ToList();
            var categories = db.TicketCategories.ToList();
            var serviceNeeded = db.ServiceNeeded.ToList();
            var serviceNeededSpaceTwin = db.ServiceNeededSpaceTwin.ToList();

            // assert
            jobTypes.Should().HaveCount(4);
            categories.Should().HaveCount(4);
            serviceNeeded.Should().HaveCount(4);
            serviceNeededSpaceTwin.Should().HaveCount(9);

            jobTypes.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.JobTypes);
            categories.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.RequestTypes);
            serviceNeeded.Where(x => x.IsActive).Should().BeEquivalentTo(expectedServiceNeeded, config => config.Excluding(x => x.LastUpdate));
            serviceNeededSpaceTwin.Should().BeEquivalentTo(expectedServiceNeededSpaceTwin, config =>
            {
                config.Excluding(x => x.LastUpdate);
                config.Excluding(x => x.ServiceNeeded);
                config.Excluding(x => x.Id);
                return config;
            });

            // extra data should be marked as inActive
            jobTypes.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraJobType);
            categories.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraCategory);
            serviceNeeded.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraServiceNeeded);
        }

    }

    [Fact]
    public async Task SyncTicketMetadataWithDuplicateData_MappedSyncMetadataService_SuccessWithNewChanges()
    {
        var ticketMetaDataResponse = Fixture.Build<TicketMetadataResponse>()
                                            .Create();

        // add duplicate data
        ticketMetaDataResponse.ServiceNeededList.Add(ticketMetaDataResponse.ServiceNeededList.First());
        ticketMetaDataResponse.JobTypes.Add(ticketMetaDataResponse.JobTypes.First());
        ticketMetaDataResponse.RequestTypes.Add(ticketMetaDataResponse.RequestTypes.First());
        var spaceTwinDict = new Dictionary<string, string>();
        var dtResponse = new List<BuildingsTwinDto>();

        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            // build the spaceTwinDict to be used in the expected digitTwin response
            spaceTwinDict.Add(item.SpaceId.ToString("N"), $"twin-{item.SpaceId}");
        }

        // expected results
        var expectedServiceNeeded = new List<ServiceNeededEntity>();
        foreach (var item in ticketMetaDataResponse.ServiceNeededList.DistinctBy(x => x.Id))
        {
            expectedServiceNeeded.Add(new ServiceNeededEntity
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.RequestTypeId
            });
        }

        // set up the  response of space service needed list
        // the space id and service needed mush match the digit twin response and service needed list ids
        var spaceSpaceServiceNeededList = new List<SpaceServiceNeeded>();
        foreach (var item in spaceTwinDict)
        {
            var serviceNeededIdsList = ticketMetaDataResponse.ServiceNeededList.DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            spaceSpaceServiceNeededList.Add(new SpaceServiceNeeded(new Guid(), Guid.Parse(item.Key), serviceNeededIdsList));

        }
        ticketMetaDataResponse.SpaceServiceNeededList = spaceSpaceServiceNeededList;

        // set up the expected results of service needed space twin
        var expectedServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in ticketMetaDataResponse.SpaceServiceNeededList)
        {
            foreach (var serviceNeededId in item.ServiceNeededIds)
            {
                expectedServiceNeededSpaceTwin.Add(new ServiceNeededSpaceTwinEntity
                {
                    ServiceNeededId = serviceNeededId,
                    SpaceTwinId = spaceTwinDict.GetValueOrDefault(item.SpaceId.ToString("N"))
                });
            }

        }
        // remove the first space service needed from the expected results
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var logger = new Mock<ILogger<MappedSyncMetadataService>>();
            var httpFactory = new Mock<IHttpClientFactory>();


            var mappedApiServiceMock = new Mock<IMappedApiService>();
            var dtApiMock = new Mock<IDigitalTwinServiceApi>();
            var cacheMock = new Mock<IAppCache>();

            mappedApiServiceMock.Setup(x => x.GetTicketMetaDataAsync(It.IsAny<MappedIntegrationConfiguration>()))
                                .ReturnsAsync(ticketMetaDataResponse);
            cacheMock.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, Task<Dictionary<string, string>>>>()))
                     .ReturnsAsync(spaceTwinDict);

            var service = new MappedSyncMetadataService(httpFactory.Object,
                                                        logger.Object,
                                                        db,
                                                        configuration,
                                                        mappedApiServiceMock.Object,
                                                        dtApiMock.Object,
                                                        cacheMock.Object);

            // existing data in database
            var existingJobTypes = ticketMetaDataResponse.JobTypes.Select(x => new JobTypeEntity
            {
                Id = x.Id,
                Name = x.Name
            }).FirstOrDefault();
            // extra job type that should be marked as inActive after sync
            var extraJobType = new JobTypeEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Job Type",
                IsActive = true
            };
            var existingCategories = ticketMetaDataResponse.RequestTypes.Select(x => new TicketCategoryEntity
            {
                Id = x.Id,
                Name = x.Name
            }).FirstOrDefault();
            // extra category that should be marked as inActive after sync
            var extraCategory = new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Category",
                IsActive = true
            };
            var existingServiceNeeded = ticketMetaDataResponse.ServiceNeededList.Select(x => new ServiceNeededEntity
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.RequestTypeId
            }).FirstOrDefault();
            // extra service needed that should be marked as inActive after sync
            var extraServiceNeeded = new ServiceNeededEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extra Service Needed",
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };
            var existingServiceNeededSpaceTwin = expectedServiceNeededSpaceTwin.FirstOrDefault();
            // extra service needed space twin that should be marked as deleted after sync
            var extraServiceNeededSpaceTwin = new ServiceNeededSpaceTwinEntity
            {
                Id = Guid.NewGuid(),
                ServiceNeededId = Guid.NewGuid(),
                SpaceTwinId = expectedServiceNeededSpaceTwin.Select(x => x.SpaceTwinId).FirstOrDefault()
            };
            db.JobTypes.Add(existingJobTypes);
            db.JobTypes.Add(extraJobType);
            db.TicketCategories.Add(existingCategories);
            db.TicketCategories.Add(extraCategory);
            db.ServiceNeeded.Add(existingServiceNeeded);
            db.ServiceNeeded.Add(extraServiceNeeded);
            db.ServiceNeededSpaceTwin.Add(existingServiceNeededSpaceTwin);
            db.ServiceNeededSpaceTwin.Add(extraServiceNeededSpaceTwin);
            db.SaveChanges();
            await service.SyncTicketMetadata();
            var jobTypes = db.JobTypes.ToList();
            var categories = db.TicketCategories.ToList();
            var serviceNeeded = db.ServiceNeeded.ToList();
            var serviceNeededSpaceTwin = db.ServiceNeededSpaceTwin.ToList();

            // assert
            jobTypes.Should().HaveCount(4);
            categories.Should().HaveCount(4);
            serviceNeeded.Should().HaveCount(4);
            serviceNeededSpaceTwin.Should().HaveCount(9);

            jobTypes.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.JobTypes.DistinctBy(x => x.Id));
            categories.Where(x => x.IsActive).Should().BeEquivalentTo(ticketMetaDataResponse.RequestTypes.DistinctBy(x => x.Id));
            serviceNeeded.Where(x => x.IsActive).Should().BeEquivalentTo(expectedServiceNeeded, config => config.Excluding(x => x.LastUpdate));
            serviceNeededSpaceTwin.Should().BeEquivalentTo(expectedServiceNeededSpaceTwin, config =>
            {
                config.Excluding(x => x.LastUpdate);
                config.Excluding(x => x.ServiceNeeded);
                config.Excluding(x => x.Id);
                return config;
            });

            // extra data should be marked as inActive
            jobTypes.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraJobType);
            categories.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraCategory);
            serviceNeeded.Where(x => !x.IsActive).First().Should().BeEquivalentTo(extraServiceNeeded);
        }

    }
}



