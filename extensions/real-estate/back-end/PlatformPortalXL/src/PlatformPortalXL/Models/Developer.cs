using System;

namespace PlatformPortalXL.Models
{
    public class Developer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}
