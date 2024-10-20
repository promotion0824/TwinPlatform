using System;

namespace DirectoryCore.Configs
{
    public class SingleTenantOptions
    {
        public bool IsSingleTenant { get; set; }
        public Guid CustomerUserIdForGroupUser { get; set; }
    }
}
