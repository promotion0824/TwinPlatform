using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

namespace Willow.AzureDigitalTwins.Api.Persistence;

public class ContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{
    private readonly ILogger _logger;

    public ContextFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ContextFactory()
    {
        _logger = LoggerFactory.Create(config =>
        {
            config.AddConsole();
        }).CreateLogger<ContextFactory<TContext>>();
    }

    public async Task Main()
    {
        await MigrateAsync();
    }

    public async Task<bool> MigrateAsync(IConfiguration? configuration = null)
    {
        try
        {
            configuration ??= GetConfiguration();

            var runMigrations = configuration.GetValue<bool>("RunMigrations");
            _logger.LogInformation("Application is set to Run Database Migration: {RunMigrations}", runMigrations);
            if (!runMigrations)
                return false;

            await using var context = CreateDbContext(configuration);
            if (context != null)
                await DbSeed.Initialize(context);
            else
            {
                throw new ArgumentException("Invalid SQL Database");
            }
            _logger.LogTrace($"{context.Database.ProviderName}  migration completed.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            return false;
        }
    }

    public TContext CreateDbContext(string[] args)
    {
        var configuration = GetConfiguration();
        return CreateDbContext(configuration);
    }

    //TODO: Instead of having separate methods for all DBContext, we can use the activator to create instance
    //Activator.CreateInstance(typeof(TContext), optionsBuilder.Options) as TContext;
    //CreateDbContext method can also take Database Name as the parameter instead to get the conn. string.
    public TContext? CreateDbContext(IConfiguration configuration)
    {
        if (typeof(TContext).Name == "JobsContext")
        {
            _logger.LogInformation("Initializing JobsDB Database migration.");

            return CreateJobsDbContext(configuration) as TContext;
        }
        else if (typeof(TContext).Name == "MappingContext")
        {
            _logger.LogInformation("Initializing MappingDB Database migration.");

            return CreateMappingDbContext(configuration) as TContext;
        }
        return null;
    }

    private JobsContext CreateJobsDbContext(IConfiguration configuration)
    {
        _logger.LogTrace("Creating Jobs DB Context");

        try
        {
            var connectionString = configuration.GetDbConnectionString("TwinsApiDb");

            var optionsBuilder = new DbContextOptionsBuilder<JobsContext>()
                .UseSqlServer(connectionString,
                    opts =>
                    {
                        opts.MigrationsAssembly("Willow.AzureDigitalTwins.Api.Persistence");
                        opts.EnableRetryOnFailure();
                    });

            return new JobsContext(optionsBuilder.Options);
        }
        catch (Exception)
        {
            _logger.LogError("Error while Creating Jobs DB Context.");
            throw;
        }
    }

    private MappingContext CreateMappingDbContext(IConfiguration configuration)
    {
        _logger.LogTrace("Creating Mapping DB Context");

        try
        {
            var connectionString = configuration.GetConnectionString("MappingDb");

            var optionsBuilder = new DbContextOptionsBuilder<MappingContext>()
                .UseSqlServer(connectionString,
                    opts =>
                    {
                        opts.MigrationsAssembly("Willow.AzureDigitalTwins.Api.Persistence");
                        opts.EnableRetryOnFailure();
                    });

            return new MappingContext(optionsBuilder.Options);
        }
        catch (Exception)
        {
            _logger.LogError("Error while Creating Mapping DB Context.");
            throw;
        }
    }

    private IConfiguration GetConfiguration()
    {
        _logger.LogTrace("Getting Database Connection string.");

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.migrator.json", optional: false)
            .AddEnvironmentVariables();

        return configurationBuilder.Build();
    }

}
