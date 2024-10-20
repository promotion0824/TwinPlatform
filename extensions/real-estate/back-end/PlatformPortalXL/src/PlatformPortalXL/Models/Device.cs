using System;

namespace PlatformPortalXL.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public Guid ConnectorId { get; set; }
        public string Name { get; set; }
        public bool IsDetected { get; set; }
        public bool IsEnabled { get; set; }
    }
}
