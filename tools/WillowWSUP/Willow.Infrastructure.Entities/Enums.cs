namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Enum for Customer Status.
    /// </summary>
    internal enum CustomerStatusEnum
    {
        /// <summary>
        /// Inactive Customer.
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Active Customer.
        /// </summary>
        Active = 2,
    }

    /// <summary>
    /// Enum for Customer Instance Status.
    /// </summary>
    internal enum CustomerInstanceStatusEnum
    {
        /// <summary>
        /// The customer instance is still being commissioned.
        /// </summary>
        Commissioning = 1,

        /// <summary>
        /// The customer instance is active.
        /// </summary>
        Active = 2,

        /// <summary>
        /// The customer instance has been decommissioned.
        /// </summary>
        Decommissioned = 3,
    }

    /// <summary>
    /// Enum for Application Status.
    /// </summary>
    internal enum ApplicationStatusEnum
    {
        /// <summary>
        /// Inactive Application.
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// The application is a preview.
        /// </summary>
        Preview = 2,

        /// <summary>
        /// Active Application.
        /// </summary>
        Active = 3,
    }

    /// <summary>
    /// Enum for Building Connector Status.
    /// </summary>
    internal enum BuildingConnectorStatusEnum
    {
        /// <summary>
        /// The connector is being commissioned for this building.
        /// </summary>
        Commissioning = 1,

        /// <summary>
        /// Active Connector.
        /// </summary>
        Active = 2,

        /// <summary>
        /// The Building Connector is intentionally offline.
        /// </summary>
        Offline = 3,

        /// <summary>
        /// The Building Connector has been disabled.
        /// </summary>
        Disabled = 4,
    }

    /// <summary>
    /// Enum for Connector Status.
    /// </summary>
    internal enum ConnectorStatusEnum
    {
        /// <summary>
        /// The connector is in development.
        /// </summary>
        InDevelopment = 1,

        /// <summary>
        /// Active Connector.
        /// </summary>
        Active = 2,

        /// <summary>
        /// Inactive Connector.
        /// </summary>
        Inactive = 3,
    }

    /// <summary>
    /// Enum for Customer Instance Application Status.
    /// </summary>
    internal enum CustomerInstanceApplicationStatusEnum
    {
        /// <summary>
        /// Inactive Application.
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Active Application.
        /// </summary>
        Active = 2,
    }

    internal enum TeamEnum
    {
        CloudOps = 1,
        ActivateTechnology = 2,
        AdvancedAnalytics = 3,
        Connectors = 4,
        CoreServices = 5,
        Dashboards = 6,
        InvestaExperience = 7,
        IoTServices = 8,
        SearchAndExplore = 9,
        Security = 10,
        Workflows = 11,
        Unknown = 999999,
    }
}
