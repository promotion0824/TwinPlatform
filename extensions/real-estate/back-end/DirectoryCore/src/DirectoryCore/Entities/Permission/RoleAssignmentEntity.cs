using System;
using System.ComponentModel.DataAnnotations.Schema;
using DirectoryCore.Enums;
using Willow.Directory.Models;

namespace DirectoryCore.Entities.Permission
{
    public class RoleAssignmentEntity : RoleAssignment
    {
        public Guid CustomerId { get; set; }
        public Guid PortfolioId { get; set; }
    }
}
