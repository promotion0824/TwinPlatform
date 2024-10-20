#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a customer instance.
    /// </summary>
    public class CustomerInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerInstance"/> class.
        /// </summary>
        public CustomerInstance()
        {
            CustomerInstanceApplications = new HashSet<CustomerInstanceApplication>();
            Buildings = new HashSet<Building>();
            Connectors = new HashSet<Connector>();
        }

        /// <summary>
        /// Gets the identifier of the customer instance.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the identifier of the customer.
        /// </summary>
        public Guid CustomerId { get; init; }

        /// <summary>
        /// Gets the name of the customer instance.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the short name of the customer instance.
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// Gets the domain of the customer instance.
        /// </summary>
        public string Domain { get; init; }

        /// <summary>
        /// Gets the Full domain of the customer instance.
        /// </summary>
        public string FullDomain { get; init; }

        /// <summary>
        /// Gets the DNS Environment Suffix of the customer instance.
        /// </summary>
        public string DnsEnvSuffix { get; init; }

        /// <summary>
        /// Gets the DisplayName of the customer instance.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the customer instance status identifier.
        /// </summary>
        public int CustomerInstanceStatusId { get; init; }

        /// <summary>
        /// Gets the full name of the customer instance.
        /// </summary>
        public string FullCustomerInstanceName { get; init;}

        /// <summary>
        /// Gets the deployment phase of the customer instance.
        /// </summary>
        public string DeploymentPhase { get; init; }

        /// <summary>
        /// Gets the lifecycle state
        /// </summary>
        public string LifecycleState { get; init; }

        /// <summary>
        /// Gets the resource group name of the customer instance.
        /// </summary>
        public string ResourceGroupName { get; init; }

        /// <summary>
        /// Gets the Azure Digital Twins Instance for the Customer Instance.
        /// </summary>
        public string AzureDigitalTwinsInstance { get; init; }

        /// <summary>
        /// Gets the Azure Data Explorer Instance for the Customer Instance.
        /// </summary>
        public string AzureDataExplorerInstance { get; init; }

        /// <summary>
        /// Gets the stamp identifier.
        /// </summary>
        public Guid StampId { get; init; }

        /// <summary>
        /// Gets the customer associated with the customer instance.
        /// </summary>
        public virtual Customer Customer { get; init; }

        /// <summary>
        /// Gets the status of the customer instance.
        /// </summary>
        public virtual CustomerInstanceStatus CustomerInstanceStatus { get; init; }

        /// <summary>
        /// Gets the stamp associated with the customer instance.
        /// </summary>
        public virtual Stamp Stamp { get; init; }

        /// <summary>
        /// Gets the applications associated with the customer instance.
        /// </summary>
        public virtual ICollection<CustomerInstanceApplication> CustomerInstanceApplications { get; init; }

        /// <summary>
        /// Gets the buildings associated with the customer instance.
        /// </summary>
        public virtual ICollection<Building> Buildings { get; init; }

        /// <summary>
        /// Gets the connectors associated with the customer instance.
        /// </summary>
        public virtual ICollection<Connector> Connectors { get; init; }
    }
}
