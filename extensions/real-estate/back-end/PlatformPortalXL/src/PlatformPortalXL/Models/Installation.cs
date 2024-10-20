using System;

namespace PlatformPortalXL.Models
{
    public class Installation
    {
        public Guid AppId { get; set; }
        public string InstalledVersion { get; set; }
        public DateTime InstalledDate { get; set; }
        public Guid InstalledBy { get; set; }
    }
}
