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

public sealed class ModuleDataServiceTests : IDisposable
{
    private const string _dbName = "DashboardDataAccessModuleDataServiceTestDb";

    public void Dispose()
    {
        DbContextHelper.DropTestDb(_dbName);
    }

    [Theory]
    [AutoData]
    public async Task GetAsync_NoIdMatch_ShouldReturnNull(Guid id)
    {
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new ModuleDataService(context);

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
            SiteId = Guid.NewGuid()
        };
        var deployment = new DeploymentEntity
        {
            Id = Guid.NewGuid(),
            Module = module
        };
        var moduleConfig = new ModuleConfigEntity
        {
            Module = module,
            IsAutoDeployment = true,
            IoTHubName = "IoTHubName",
            DeviceName = "DeviceName",
            Environment = @"{""test"": ""value""}"
        };
        module.Config = moduleConfig;
        await prepContext.Modules.AddAsync(module);
        await prepContext.Deployments.AddAsync(deployment);
        await prepContext.ModuleConfigs.AddAsync(moduleConfig);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        var expected = ModuleDto.CreateFrom(module, deployment);

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);

        var service = new ModuleDataService(context);
        var result = await service.GetAsync(module.Id);

        result.Should()
              .Be(expected);
    }

    [Fact]
    public async Task UpsertAsync_NullId_ShouldReturnWithNewId()
    {
        var input = new ModuleUpsertInput(Guid.NewGuid(),
                                          "name",
                                          "type",
                                          true);

        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new ModuleDataService(context);

        var result = await service.UpsertAsync(input);

        result.Id.Should()
              .NotBeEmpty();
        result.SiteId.Should()
              .Be(input.SiteId);
        result.Name.Should()
              .Be(input.Name);
        result.ModuleType.Should()
              .Be(input.ModuleType);
    }

    [Fact]
    public async Task UpsertAsync_NewId_ShouldReturnWithNewId()
    {
        var input = new ModuleUpsertInput(Guid.NewGuid(),
                                          "name",
                                          "type",
                                          true,
                                          true,
                                          Guid.NewGuid());

        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new ModuleDataService(context);

        var result = await service.UpsertAsync(input);

        result.Id.Should()
              .Be(input.Id!.Value);
        result.SiteId.Should()
              .Be(input.SiteId);
        result.Name.Should()
              .Be(input.Name);
        result.ModuleType.Should()
              .Be(input.ModuleType);
    }

    [Fact]
    public async Task UpsertAsync_ExistingId_ShouldUpdateExisting()
    {
        var input = new ModuleUpsertInput(Guid.NewGuid(),
                                          "name",
                                          "type",
                                          true,
                                          true,
                                          Guid.NewGuid());

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);
        await prepContext.Modules.AddAsync(new ModuleEntity
        {
            Id = input.Id!.Value,
            SiteId = Guid.NewGuid()
        });

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);

        var result = await service.UpsertAsync(input);

        result.Id.Should()
              .Be(input.Id!.Value);
        result.SiteId.Should()
              .Be(input.SiteId);
        result.Name.Should()
              .Be(input.Name);
        result.ModuleType.Should()
              .Be(input.ModuleType);
    }

    [Theory]
    [AutoData]
    public async Task UpdateConfiguration_ModuleNotExist_ShouldThrowException(ModuleUpdateConfigurationInput input)
    {
        await using var context = DbContextHelper.GetContextWithFreshDb(_dbName);
        var service = new ModuleDataService(context);

        var action = async () => await service.UpdateConfigurationAsync(input);
        await action.Should()
                    .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateConfiguration_NewConfig_ShouldReturnWithNewConfig()
    {
        var input = new ModuleUpdateConfigurationInput(Guid.NewGuid(),
                                                       true,
                                                       "deviceName",
                                                       "IoTHubName",
                                                       @"{""test"": ""value""}");
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);
        await prepContext.Modules.AddAsync(new ModuleEntity
        {
            Id = input.Id,
            SiteId = Guid.NewGuid()
        });
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);

        var result = await service.UpdateConfigurationAsync(input);
        result.Id.Should()
              .Be(input.Id);
        result.Environment.Should()
              .Be(input.Environment);
        result.DeviceName.Should()
              .Be(input.DeviceName);
        result.IoTHubName.Should()
              .Be(input.IoTHubName);
        result.IsAutoDeployment.Should()
              .Be(input.IsAutoDeployment);
    }

    [Fact]
    public async Task UpdateConfiguration_ExistingConfig_ShouldUpdateExisting()
    {
        var input = new ModuleUpdateConfigurationInput(Guid.NewGuid(),
                                                       true,
                                                       "deviceName",
                                                       "IoTHubName",
                                                       @"{""test"": ""value""}");
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);
        await prepContext.Modules.AddAsync(new ModuleEntity
        {
            Id = input.Id,
            SiteId = Guid.NewGuid()
        });
        await prepContext.ModuleConfigs.AddAsync(new ModuleConfigEntity { ModuleId = input.Id });
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);

        var result = await service.UpdateConfigurationAsync(input);
        result.Id.Should()
              .Be(input.Id);
        result.Environment.Should()
              .Be(input.Environment);
        result.DeviceName.Should()
              .Be(input.DeviceName);
        result.IoTHubName.Should()
              .Be(input.IoTHubName);
        result.IsAutoDeployment.Should()
              .Be(input.IsAutoDeployment);
    }

    [Fact]
    public async Task SearchAsync_NullSearchTerm_ShouldReturnPageWithAllCount()
    {
        var input = new ModuleSearchInput(null,
                                          Page: 1,
                                          PageSize: 23);

        var modulesToAdd = new List<ModuleEntity>();
        for (var i = 0; i < 100; i++)
        {
            var module = new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid()
            };
            modulesToAdd.Add(module);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(100);
        result.Items.Count()
              .Should()
              .Be(23);
    }

    [Fact]
    public async Task SearchAsync_SearchByName_ShouldReturnPageWithSearchedCount()
    {
        var input = new ModuleSearchInput("A",
                                          Page: 1,
                                          PageSize: 23);

        var modulesToAdd = new List<ModuleEntity>();
        for (var i = 0; i < 100; i++)
        {
            var name = (i % 3) switch
            {
                0 => "aa",
                1 => "ab",
                _ => "b"
            };
            var module = new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid(),
                Name = name
            };
            modulesToAdd.Add(module);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(67);
        result.Items.Count()
              .Should()
              .Be(23);
    }
    
    [Fact]
    public async Task SearchAsync_SearchByModuleType_ShouldReturnPageWithSearchedCount()
    {
        var input = new ModuleSearchInput(ModuleType: "A",
                                          Page: 1,
                                          PageSize: 23);

        var modulesToAdd = new List<ModuleEntity>();
        for (var i = 0; i < 100; i++)
        {
            var moduleType = (i % 3) switch
            {
                0 => "aa",
                1 => "ab",
                _ => "b"
            };
            var module = new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid(),
                ModuleType = moduleType
            };
            modulesToAdd.Add(module);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(67);
        result.Items.Count()
              .Should()
              .Be(23);
    }
    
    [Fact]
    public async Task SearchAsync_SearchByDeviceName_ShouldReturnPageWithSearchedCount()
    {
        var modulesToAdd = new List<ModuleEntity>();
        var modules1 = new List<ModuleEntity>();
        var modules2 = new List<ModuleEntity>();
        for (var i = 0; i < 99; i++)
        {
            var moduleType = "moduleType";
            var module = new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid(),
                ModuleType = moduleType
            };
            switch (i % 2)
            {
                case 0:
                    modules1.Add(module);
                    break;
                default:
                    modules2.Add(module);
                    break;
            }
            modulesToAdd.Add(module);
        }

        var configs1 = modules1.Select(module => new ModuleConfigEntity
        {
            ModuleId = module.Id,
            DeviceName = "config1"
        });

        var configs2 = modules2.Select(module => new ModuleConfigEntity
        {
            ModuleId = module.Id,
        });

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.ModuleConfigs.AddRangeAsync(configs1);
        await prepContext.ModuleConfigs.AddRangeAsync(configs2);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var input = new ModuleSearchInput(DeviceName: "config",
                                          Page: 1,
                                          PageSize: 23);

        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(50);
        result.Items.Count()
              .Should()
              .Be(23);
    }

    [Fact]
    public async Task SearchAsync_SearchByDeploymentIds_ShouldReturnPageWithSearchedCount()
    {
        var deploymentIdToSearch = Guid.NewGuid();
        var deploymentIdsToSearch = new List<Guid> { deploymentIdToSearch };
        for (var i = 0; i < 10; i++)
        {
            deploymentIdsToSearch.Add(Guid.NewGuid());
        }

        var input = new ModuleSearchInput(DeploymentIds: deploymentIdsToSearch,
                                          Page: 1,
                                          PageSize: 23);

        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            Name = "testName"
        };
        var modulesToAdd = new List<ModuleEntity> { module };
        for (var i = 0; i < 20; i++)
        {
            modulesToAdd.Add(new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid(),
                Name = "testName"
            });
        }

        var deployment = new DeploymentEntity
        {
            Id = deploymentIdToSearch,
            ModuleId = module.Id
        };

        var deploymentsToAdd = new List<DeploymentEntity> { deployment };
        for (var i = 0; i < 100; i++)
        {
            deploymentsToAdd.Add(new DeploymentEntity
            {
                Id = Guid.NewGuid(),
                ModuleId = module.Id
            });
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.Deployments.AddRangeAsync(deploymentsToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.SearchAsync(input);

        result.TotalCount.Should()
              .Be(1);
        var itemList = result.Items.ToList();
        itemList.Count.Should()
                .Be(1);
        itemList.First()
                .Deployments!.Count()
                .Should()
                .Be(1);
    }
    
    [Fact]
    public async Task GetModuleTypesAsync_SearchByName_ShouldReturnModuleType()
    {
        var modulesToAdd = new List<ModuleEntity>();
        for (var i = 0; i < 100; i++)
        {
            var moduleType = (i % 3) switch
            {
                0 => "aa",
                1 => "ab",
                _ => "b"
            };
            var module = new ModuleEntity
            {
                Id = Guid.NewGuid(),
                SiteId = Guid.NewGuid(),
                ModuleType = moduleType
            };
            modulesToAdd.Add(module);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.Modules.AddRangeAsync(modulesToAdd);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.GetModuleTypesAsync(new ModuleTypesSearchInput("aa"));

        result.TotalCount
              .Should()
              .Be(1);
        result.Items.Count()
              .Should()
              .Be(1);
    }
    
    [Fact]
    public async Task GetModuleTypeVersionsAsync_GetByModuleTypeName_ShouldReturnAllVersions()
    {
        var versions = new List<ModuleTypeVersionEntity>();
        for (var i = 0; i < 100; i++)
        {
            var moduleType = (i % 3) switch
            {
                0 => "aa",
                1 => "ab",
                _ => "b"
            };
            var item = new ModuleTypeVersionEntity
            {
                Id = Guid.NewGuid(),
                Version = i.ToString(),
                ModuleType = moduleType
            };
            versions.Add(item);
        }

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);

        await prepContext.ModuleTypeVersions.AddRangeAsync(versions);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = await service.GetModuleTypeVersionsAsync("aa");

        result.Count()
              .Should()
              .Be(34);
    }
    
    [Fact]
    public async Task AddModuleTypeVersionAsync_AddNewVersion_ShouldReturnListContainNewVersion()
    {
        var moduleType = "aa";
        var existingVersions = new [] {"1", "2", "3"};
        var newVersion = "4";

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);
        foreach (var version in existingVersions)
        {
            prepContext.ModuleTypeVersions.Add(new ModuleTypeVersionEntity
            {
                Id = Guid.NewGuid(),
                Version = version,
                ModuleType = "aa"
            });
        }
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = (await service.AddModuleTypeVersionsAsync(moduleType, newVersion)).ToList();

        result.Count
              .Should()
              .Be(4);

        result.Should()
              .Contain("4");
    }
    
    [Fact]
    public async Task AddModuleTypeVersionAsync_AddExistingVersion_ShouldBeUnchanged()
    {
        var moduleType = "aa";
        var existingVersions = new [] {"1", "2", "3"};
        var newVersion = "3";

        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName);
        foreach (var version in existingVersions)
        {
            prepContext.ModuleTypeVersions.Add(new ModuleTypeVersionEntity
            {
                Id = Guid.NewGuid(),
                Version = version,
                ModuleType = "aa"
            });
        }
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var service = new ModuleDataService(context);
        var result = (await service.AddModuleTypeVersionsAsync(moduleType, newVersion)).ToList();

        result.Should()
              .BeEquivalentTo(existingVersions);
    }
}