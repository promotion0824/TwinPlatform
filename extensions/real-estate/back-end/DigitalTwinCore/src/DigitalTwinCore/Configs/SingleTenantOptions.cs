using System;

namespace DigitalTwinCore.Configs
{
    // This class exists in several services, should be moved to a common library.
    public class SingleTenantOptions
    {
        public bool IsSingleTenant { get; set; }
        public Guid CustomerUserIdForGroupUser { get; set; }
    }
}
