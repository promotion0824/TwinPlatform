using System;

namespace Willow.Directory.Models
{
    public class RoleAssignment
    {
        public Guid PrincipalId          { get; set; }
        public Guid RoleId               { get; set; }
        public Guid ResourceId           { get; set; }
        public RoleResourceType ResourceType { get; set; }
    }

    public enum RoleResourceType
    {
        Customer  = 1,
        Portfolio = 2,
        Site      = 3
    }
}
