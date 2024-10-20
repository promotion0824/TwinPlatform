using System;
using System.Collections.Generic;
using System.Text;

using Willow.Directory.Models;

namespace Willow.Management
{
    public class RoleAssignmentDto : RoleAssignment
    {
        public Guid? CustomerId  { get; set; }
        public Guid? PortfolioId { get; set; }
    }
}
