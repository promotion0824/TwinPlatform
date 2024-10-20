namespace Connector.XL.Infrastructure.HealthCheck;

using System.Collections.Generic;

internal class HealthCheckSettings
{
    public List<DatabaseSettings> Databases { get; set; }

    public string[] Apis { get; set; }
}

internal class DatabaseSettings
{
    public string ConnectionString { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }
}
