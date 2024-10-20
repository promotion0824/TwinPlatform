namespace Willow.AppContext;

#nullable disable

using System.Reflection;

/// <summary>
/// The contextual information for the application.
/// </summary>
public class WillowContextOptions
{
    private string appName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
    private string replicaName = Environment.GetEnvironmentVariable("CONTAINER_APP_REPLICA_NAME") ?? "01";

    /// <summary>
    /// Initializes a new instance of the <see cref="WillowContextOptions"/> class.
    /// </summary>
    public WillowContextOptions()
    {
        AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty;
        EnvironmentConfiguration = new EnvironmentConfigurationOptions();
        RegionConfiguration = new RegionConfigurationOptions();
        StampConfiguration = new StampConfigurationOptions();
        CustomerInstanceConfiguration = new CustomerInstanceConfiguration();

        var meterName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
        MeterOptions = new WillowMeterOptions
        {
            Name = meterName,
            Version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown",
            Tags = Values,
        };
    }

    /// <summary>
    /// Gets or sets the version of the application.
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment configuration.
    /// </summary>
    public EnvironmentConfigurationOptions EnvironmentConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the region configuration.
    /// </summary>
    public RegionConfigurationOptions RegionConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the stamp configuration.
    /// </summary>
    public StampConfigurationOptions StampConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the customer instance configuration.
    /// </summary>
    public CustomerInstanceConfiguration CustomerInstanceConfiguration { get; set; }

    /// <summary>
    /// Gets the meter options.
    /// </summary>
    public WillowMeterOptions MeterOptions { get; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string AppName
    {
        get => appName;
        set => appName = value;
    }

    /// <summary>
    /// Gets or sets the replica name.
    /// </summary>
    public string ReplicaName
    {
        get => replicaName;
        set => replicaName = value;
    }

    /// <summary>
    /// Gets the application role instance id.
    /// </summary>
    public string AppRoleInstanceId
    {
        get => $"{FullCustomerInstanceName}:{AppName}:{ReplicaName}";
    }

    /// <summary>
    /// Gets the full customerInstanceName.
    /// </summary>
    public string FullCustomerInstanceName
    {
        get => $"{EnvironmentConfiguration.ShortName}:{RegionConfiguration.ShortName}:{StampConfiguration.Name}:{CustomerInstanceConfiguration.CustomerInstanceName}";
    }

    /// <summary>
    /// Gets a List of Key-Value Pairs of all configuration settings for the object needed for adding to metrics and logs as dimensions.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> Values
    {
        get
        {
            var kvps = new List<KeyValuePair<string, object>>
                {
                    new("AppVersion", AppVersion),
                    new("AppRoleName", AppName),
                    new("AppRoleInstance", AppRoleInstanceId),
                    new("FullCustomerInstanceName", FullCustomerInstanceName),
                };

            return kvps;
        }
    }

    /// <summary>
    /// Gets the role instance id.
    /// </summary>
    /// <param name="role">The role name.</param>
    /// <param name="instanceNumber">The instance number.</param>
    /// <returns>A colon separated formatted string which allows the log user to parse important dimensions from the RoleInstanceId.</returns>
    [Obsolete("Use AppRoleInstanceId instead.")]
    public string GetRoleInstanceId(string role, string instanceNumber)
    {
        return $"{EnvironmentConfiguration.ShortName}:{RegionConfiguration.ShortName}:{StampConfiguration.Name}:{CustomerInstanceConfiguration.CustomerInstanceName}:{role}:{instanceNumber}";
    }
}
