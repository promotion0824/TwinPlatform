using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Willow.CommandAndControl.Application.Services;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data;

namespace Willow.CommandAndControl.Tests.Helpers;

internal class CommandInitializer
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly IActivityLogger _activityLoggerMock = Substitute.For<IActivityLogger>();

    public CommandInitializer()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public ICommandManager GetCommandManager(ApplicationDbContext dbContext)
    {
        var logger = Substitute.For<ILogger<CommandManager>>();
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var mappedMock = new Mock<IMappedGatewayService>();
        var dbTransactionsMock = new Mock<IDbTransactions>();
        return new CommandManager(logger,
                                dbContext,
                                dbTransactionsMock.Object,
                                mappedMock.Object,
                                conflictResolver);
    }

    public ApplicationDbContext GetApplicationDbContext()
    {
        var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
        var userInfoService = new UserInfoService(httpContextAccessor);
        var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
        var dbContext = new ApplicationDbContext(_dbContextOptions, interceptor);
        return dbContext;
    }
}
