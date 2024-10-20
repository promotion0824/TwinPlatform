using Microsoft.Extensions.Configuration;

namespace Authorization.TwinPlatform.Persistence.Extensions;

/// <summary>
/// Extension class for DB Context configurations
/// </summary>
public static class ConfigurationExtensions
{
	public static string GetAuthorizationDbConnectionString(this IConfiguration configuration)
	{
		return configuration.GetConnectionString("AuthorizationDb");
	}
}