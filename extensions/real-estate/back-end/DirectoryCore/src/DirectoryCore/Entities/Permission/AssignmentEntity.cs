using System;
using System.ComponentModel.DataAnnotations.Schema;
using DirectoryCore.Enums;
using Willow.Directory.Models;

namespace DirectoryCore.Entities.Permission
{
    [Table("Assignments")]
    public class AssignmentEntity
    {
        public Guid PrincipalId { get; set; }
        public PrincipalType PrincipalType { get; set; }
        public Guid RoleId { get; set; }
        public Guid ResourceId { get; set; }
        public RoleResourceType ResourceType { get; set; }
    }
}
