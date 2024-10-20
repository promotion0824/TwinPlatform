using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Willow.AzureDigitalTwins.Api.Persistence;

public static class ConfigurationExtensions
{
    public static string GetDbConnectionString(this IConfiguration configuration, string Db)
    {
        return configuration.GetConnectionString(Db);
    }
}
