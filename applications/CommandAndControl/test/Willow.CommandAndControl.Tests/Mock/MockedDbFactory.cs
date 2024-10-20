using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data;

namespace Willow.CommandAndControl.Tests.Mock;

internal class MockedDbFactory
{
    public static async Task<IApplicationDbContext> CreateAsync(string connectionString, bool withTestUser = true)
    {
        //mock IUserInfoService with nsubsitute
        IUserInfoService userInfoService = Substitute.For<IUserInfoService>();
        if (withTestUser)
        {
            userInfoService.GetUser().Returns(new Data.Models.User { Name = null, Email = null });
        }
        else
        {
            userInfoService.GetUser().Returns(new Data.Models.User { Name = "test", Email = "test@example.com" });
        }
        var baseEntitySaveChangesInterceptor = new BaseEntitySaveChangesInterceptor(userInfoService);

        var dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options, baseEntitySaveChangesInterceptor);
        await dbContext.Database.MigrateAsync();
        return dbContext;
    }
}
