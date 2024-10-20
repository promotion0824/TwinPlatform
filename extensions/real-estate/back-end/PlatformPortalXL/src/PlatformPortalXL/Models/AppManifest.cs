using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class AppManifest
    {
        public string ConfigurationUrl { get; set; }
        public string PostMessageUrl { get; set; }
        public List<string> Capabilities { get; set; } = new List<string>();
    }
}
