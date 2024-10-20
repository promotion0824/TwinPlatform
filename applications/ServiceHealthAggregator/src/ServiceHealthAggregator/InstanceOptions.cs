namespace Willow.ServiceHealthAggregator
{
    /// <summary>
    /// Options for the instance configuration.
    /// </summary>
    public class InstanceOptions
    {
        /// <summary>
        /// Gets a value in minutes for the refresh interval for the ScanCustomerInstanceHostedService.
        /// </summary>
        public int RefreshInterval { get; init; }

        /// <summary>
        /// Gets the address of the Twins API endpoint.
        /// </summary>
        public string? TwinsApiEndpoint { get; init; }

        /// <summary>
        /// Gets the address of the Wsup API endpoint.
        /// </summary>
        public string? WsupApiEndpoint { get; init; }

        /// <summary>
        /// Gets the Wsup API scope.
        /// </summary>
        public string? WsupApiScope { get; init; }

        /// <summary>
        /// Gets the authority for the Wsup API endpoint.
        /// </summary>
        public string? WsupApiAuthority { get; init; }

        /// <summary>
        /// Gets the client id to be used for the Wsup API endpoint.
        /// </summary>
        public string? WsupApiClientId { get; init; }

        /// <summary>
        /// Gets the customer instance id for the instance.
        /// </summary>
        public Guid CustomerInstanceId { get; init; }

        /// <summary>
        /// Gets a value indicating whether the instance scan is enabled.
        /// </summary>
        public bool EnableInstanceScan { get; init; }
    }
}
