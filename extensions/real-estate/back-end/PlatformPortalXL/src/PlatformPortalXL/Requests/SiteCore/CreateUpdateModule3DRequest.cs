using System.Collections.Generic;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class CreateUpdateModule3DRequest
    {
        public List<Module3DInfo> Modules3D { get; set; } = new List<Module3DInfo>();
    }

    public class Module3DInfo
    {
        public string Url { get; set; }

        public string ModuleName { get; set; }
    }
}
