using System;

namespace MobileXL.Models
{
    public class TicketAssignee
    {
        public TicketAssigneeType Type { get; set; }
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

        public static TicketAssignee FromCustomerUser(CustomerUser customerUser)
        {
            return new TicketAssignee
            {
                Type = TicketAssigneeType.CustomerUser,
                Id = customerUser.Id,
                FirstName = customerUser.FirstName,
                LastName = customerUser.LastName,
                Email = customerUser.Email,
                Name = $"{customerUser.FirstName} {customerUser.LastName}"
            };
        }
        public static TicketAssignee FromWorkgroup(Workgroup workgroup)
        {
            return new TicketAssignee
            {
                Type = TicketAssigneeType.WorkGroup,
                Id = workgroup.Id,
                Name = workgroup.Name
            };
        }
    }

}
