using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Willow.IoTService.Deployment.DataAccess.Db;
using Willow.IoTService.Deployment.DataAccess.Entities;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Xunit;

namespace Willow.IoTService.Deployment.DataAccess.IntegrationTests;

public sealed class AuditPropertyTests : IDisposable
{
    private const string _dbName = "DashboardDataAccessAuditPropertyTestDb";

    public void Dispose()
    {
        DbContextHelper.DropTestDb(_dbName);
    }

    [Fact]
    public async Task AddEntity_CreateWithCreatProperties_ShouldUpdateCreatedAndUpdatedProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
        };

        var userInfoService = new FakeUserInfoService();
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName, new BaseEntitySaveChangesInterceptor(userInfoService));
        await prepContext.Modules.AddAsync(module);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await Task.Delay(TimeSpan.FromMilliseconds(100));
        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var result = await context.Modules.SingleAsync(x => x.Id == module.Id);

        result.CreatedOn.Should()
              .Be(result.UpdatedOn);
        result.CreatedOn.Should()
              .BeAfter(now);
        result.CreatedBy.Should()
              .Be(userInfoService.GetUserName());
        result.UpdatedBy.Should()
              .Be(userInfoService.GetUserName());
    }
    
    [Fact]
    public async Task AddEntity_UpdateWithUpdateProperties_ShouldUpdateUpdatedProperties()
    {
        var module = new ModuleEntity
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        var userInfoService = new FakeUserInfoService();
        var prepContext = DbContextHelper.GetContextWithFreshDb(_dbName, new BaseEntitySaveChangesInterceptor(userInfoService));
        await prepContext.Modules.AddAsync(module);
        await prepContext.SaveChangesAsync();
        await prepContext.DisposeAsync();

        await Task.Delay(TimeSpan.FromMilliseconds(100));
        var userrInfoService2 = new FakeUserInfoService("updated");
        var prepContext2 = DbContextHelper.GetContextWithExistingDb(_dbName, new BaseEntitySaveChangesInterceptor(userrInfoService2));
        var updated = await prepContext2.Modules.SingleAsync(x => x.Id == module.Id);
        updated.Name = "testName";
        await prepContext2.SaveChangesAsync();
        await prepContext2.DisposeAsync();
        await using var context = DbContextHelper.GetContextWithExistingDb(_dbName);
        var result = await context.Modules.SingleAsync(x => x.Id == module.Id);

        result.UpdatedOn.Should()
              .BeAfter(module.CreatedOn);
        result.UpdatedBy.Should()
              .Be("updated");
    }
    
    private sealed class FakeUserInfoService : IUserInfoService
    {
        private readonly string _name = "TestName";

        public FakeUserInfoService(string? name = null)
        {
            if (name != null)
            {
                _name = name;
            }
        }

        public string GetUserName() => _name;
    }
}