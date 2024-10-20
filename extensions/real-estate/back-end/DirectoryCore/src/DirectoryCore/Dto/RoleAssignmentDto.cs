using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Directory.Models;

namespace DirectoryCore.Dto
{
    public class RoleAssignmentDto : RoleAssignment
    {
        public Guid? CustomerId { get; set; }
        public Guid? PortfolioId { get; set; }
    }
}
