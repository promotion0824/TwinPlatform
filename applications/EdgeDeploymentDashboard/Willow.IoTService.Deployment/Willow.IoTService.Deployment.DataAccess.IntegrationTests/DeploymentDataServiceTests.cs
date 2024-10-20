using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Willow.IoTService.Deployment.DataAccess.Entities;
using Willow.IoTService.Deployment.DataAccess.Services;
using Xunit;

namespace Willow.IoTService.Deployment.DataAccess.IntegrationTests;

public sealed class DeploymentDataServiceTests : IDisposable
{
    private const string _dbName = "DashboardDataAccessDeploymentDataServiceTestDb";

    public void Dispose()
    {
        DbContextHelper.DropTestDb(_dbName);
    }

    [Theory]
    [AutoData]
    public async Task GetAsync_NoIdMatch_ShouldReturnNull(Guid id)
    {
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new DeploymentDataService(context);

        var result = await service.GetAsync(id);

        result.Should()
              .BeNull();
    }

    [Fact]
    public async Task GetAsync_IdMatch_ShouldReturnEntity()
    {
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            Name = "TestName",
            ModuleType = "TestType"
        };
        var entity = new DeploymentEntity
        {
            Id = Guid.NewGuid(),
            Module = module
        };
        await prepContext.Modules.AddAsync(module);
        await prepContext.Deployments.AddAsync(entity);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        var expected = DeploymentDto.CreateFrom(entity);

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var result = await service.GetAsync(entity.Id);

        result.Should()
              .Be(expected);
    }

    [Theory]
    [AutoData]
    public async Task CreateAsync_ModuleNoExist_ShouldThrowException(DeploymentCreateInput input)
    {
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new DeploymentDataService(context);

        var act = async () => await service.CreateAsync(input);
        await act.Should()
                 .ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public async Task CreateAsync_ValidInput_ShouldReturnCreated(DeploymentCreateInput input)
    {
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        var module = new ModuleEntity
        {
            Id = input.ModuleId,
            SiteId = Guid.NewGuid()
        };
        await prepContext.Modules.AddAsync(module);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);

        var result = await service.CreateAsync(input);
        result.Name.Should()
              .Be(result.Id.ToString("N"));
        input.Status.Should()
             .Be(result.Status);
        input.Version.Should()
             .Be(result.Version);
        input.AssignedBy.Should()
             .Be(result.AssignedBy);
        input.ModuleId.Should()
             .Be(result.ModuleId);
        input.DateTimeApplied.Should()
             .Be(result.DateTimeApplied);
    }

    [Theory]
    [AutoData]
    public async Task UpdateStatus_NoExistingDeployment_ShouldThrowException(DeploymentStatusUpdateInput input)
    {
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new DeploymentDataService(context);

        var act = async () => await service.UpdateStatusAsync(input);
        await act.Should()
                 .ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public async Task UpdateStatus_ValidInput_ShouldUpdate(DeploymentStatusUpdateInput input)
    {
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        var entity = new DeploymentEntity
        {
            Id = input.Id,
            Module = module
        };
        await prepContext.Modules.AddAsync(module);
        await prepContext.Deployments.AddAsync(entity);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var result = await service.UpdateStatusAsync(input);
        input.Status.Should()
             .Be(result.Status);
        input.Id.Should()
             .Be(result.Id);
        input.DateTimeApplied.Should()
             .Be(result.DateTimeApplied);
    }

    [Fact]
    public async Task SearchAsync_NoRecord_ShouldReturnEmpty()
    {
        var input = new DeploymentSearchInput(null,
                                              Guid.NewGuid(),
                                              null,
                                              2,
                                              23);
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new DeploymentDataService(context);

        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(0);
        result.Items.Should()
              .BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_NullSearchTerm_ShouldReturnPageWithAllCount()
    {
        var input = new DeploymentSearchInput(null,
                                              null,
                                              null,
                                              1,
                                              23);

        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        var deploymentsToAdd = new List<DeploymentEntity>();
        for (var i = 0; i < 100; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module
            };
            deploymentsToAdd.Add(entity);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddAsync(module);
        await prepContext.Deployments.AddRangeAsync(deploymentsToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(100);
        result.Items.Count()
              .Should()
              .Be(23);
    }

    [Fact]
    public async Task SearchAsync_SearchByModuleId_ShouldReturnPageForModule()
    {
        var deploymentsToAdd = new List<DeploymentEntity>();
        var module1 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        for (var i = 0; i < 50; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module1
            };
            deploymentsToAdd.Add(entity);
        }

        var module2 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        for (var i = 0; i < 60; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module2
            };
            deploymentsToAdd.Add(entity);
        }

        var input = new DeploymentSearchInput(null,
                                              module2.Id,
                                              null,
                                              1,
                                              23);

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddAsync(module1);
        await prepContext.Modules.AddAsync(module2);
        await prepContext.Deployments.AddRangeAsync(deploymentsToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(60);
        result.Items.Count()
              .Should()
              .Be(23);
    }
    
    [Fact]
    public async Task SearchAsync_SearchByDeviceName_ShouldReturnPageForModule()
    {
        var deploymentsToAdd = new List<DeploymentEntity>();
        var module1 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
        };
        var config1 = new ModuleConfigEntity
        {
            DeviceName = "config1",
            ModuleId = module1.Id
        };
        for (var i = 0; i < 50; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module1
            };
            deploymentsToAdd.Add(entity);
        }

        var module2 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        var config2 = new ModuleConfigEntity
        {
            ModuleId = module2.Id
        };
        for (var i = 0; i < 60; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module2
            };
            deploymentsToAdd.Add(entity);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddAsync(module1);
        await prepContext.Modules.AddAsync(module2);
        await prepContext.ModuleConfigs.AddAsync(config1);
        await prepContext.ModuleConfigs.AddAsync(config2);
        await prepContext.Deployments.AddRangeAsync(deploymentsToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        
        var input = new DeploymentSearchInput(null,
                                              null,
                                              "config",
                                              1,
                                              23);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(50);
        result.Items.Count()
              .Should()
              .Be(23);
    }
    
    [Fact]
    public async Task SearchAsync_SearchByIds_ShouldReturnPage()
    {
        var deploymentsToAdd = new List<DeploymentEntity>();
        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        for (var i = 0; i < 50; i++)
        {
            var entity = new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                Module = module
            };
            deploymentsToAdd.Add(entity);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddAsync(module);
        await prepContext.Deployments.AddRangeAsync(deploymentsToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var input = new DeploymentSearchInput(deploymentsToAdd.Select(x => x.Id).Take(5),
                                              null,
                                              null,
                                              1,
                                              23);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(5);
        result.Items.Count()
              .Should()
              .Be(5);
    }

    [Fact]
    public async Task CreateMultipleAsync_ValidInput_ShouldCreateMultiple()
    {
        var module1 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };
        var module2 = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddAsync(module1);
        await prepContext.Modules.AddAsync(module2);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new DeploymentDataService(context);
        var result = await service.CreateMultipleAsync(new[]
        {
            new DeploymentCreateInput(module1.Id,
                                      "",
                                      "",
                                      "",
                                      "",
                                      DateTimeOffset.UtcNow),
            new DeploymentCreateInput(module2.Id,
                                      "",
                                      "",
                                      "",
                                      "",
                                      DateTimeOffset.UtcNow),
            new DeploymentCreateInput(Guid.NewGuid(),
                                      "",
                                      "",
                                      "",
                                      "",
                                      DateTimeOffset.UtcNow)
        });

        result.Count()
              .Should()
              .Be(2);
    }
}