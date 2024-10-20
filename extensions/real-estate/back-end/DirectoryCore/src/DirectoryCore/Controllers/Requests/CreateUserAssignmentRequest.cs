using System;
using Willow.Directory.Models;

namespace DirectoryCore.Controllers.Requests
{
    public class CreateUserAssignmentRequest
    {
        public Guid RoleId { get; set; }
        public RoleResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }
    }
}
