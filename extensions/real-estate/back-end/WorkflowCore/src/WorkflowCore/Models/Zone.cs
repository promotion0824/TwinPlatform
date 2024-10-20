using System;

namespace WorkflowCore.Models
{
    public class Zone
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public bool IsArchived { get; set; }
        public ZoneStatistics Statistics { get; set; }
    }
}
