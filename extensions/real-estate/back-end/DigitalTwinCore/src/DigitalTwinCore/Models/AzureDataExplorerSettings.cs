using Willow.Api.AzureStorage;

namespace DigitalTwinCore.Models
{
    public class AzureDataExplorerSettings
    {
        // Should we deprecate this? By the looks of it, TenantId is not used at all,
        // and Name and Region are only used to form the cluster URI.
        public AzureDataExplorerClusterSettings Cluster { get; set; }
        public BlobStorageConfig BlobStorage { get; set; }

        // <summary>
        // If true, make sure the database tables, materialized views, and
        // functions exist on startup.
        // </summary>
        public bool EnsureDatabaseObjectsExist { get; set; }

        // <summary>
        // URI of the ADX cluster. Currently only used by the code that sets up
        // tables (etc) on startup, but we may want to deprecate `Cluster` in favour
        // of this.
        // </summary>
        public string ClusterUri { get; set; }

        // <summary>
        // Name of the database in the cluster. Currently only used by the code that
        // sets up tables (etc) on startup. Only makes sense in single-tenant
        // environments.
        // </summary>
        public string DatabaseName { get; set; }
    }
}
