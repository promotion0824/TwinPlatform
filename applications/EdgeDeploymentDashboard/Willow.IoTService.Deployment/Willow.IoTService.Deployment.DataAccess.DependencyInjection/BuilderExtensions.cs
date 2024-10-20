using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.DataAccess.Db;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Willow.IoTService.Deployment.DataAccess.Services;

namespace Willow.IoTService.Deployment.DataAccess.DependencyInjection;

public static class BuilderExtensions
{
    public static void ConfigureDataAccess<T>(this IServiceCollection services, string connectionString)
        where T : class, IUserInfoService
    {
        services.AddTransient<IUserInfoService, T>();
        ConfigureDataAccess(services, connectionString);
    }

    public static void ConfigureDataAccess(this IServiceCollection services, string connectionString, string serviceName)
    {
        services.AddTransient<IUserInfoService>(_ => new SimplerUserInfoService(serviceName));
        ConfigureDataAccess(services, connectionString);
    }

    private static void ConfigureDataAccess(IServiceCollection services, string connectionString)
    {
        services.AddTransient<IDeploymentDataService, DeploymentDataService>();
        services.AddTransient<IModuleDataService, ModuleDataService>();
        services.AddTransient<BaseEntitySaveChangesInterceptor>();
        services.AddDbContext<DeploymentDbContext>((serviceProvider, options) => options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(DeploymentDbContext).Assembly.FullName);
            sqlOptions.ExecutionStrategy(dependencies => new CustomAzureSqlExecutionStrategy(dependencies, 6, TimeSpan.FromSeconds(30), null, serviceProvider.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()));
        }));
        services.AddScoped<IDeploymentDbContext, DeploymentDbContext>(provider => provider.GetRequiredService<DeploymentDbContext>());
    }
}
