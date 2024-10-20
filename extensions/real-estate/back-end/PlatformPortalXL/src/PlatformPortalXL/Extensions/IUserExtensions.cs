using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL
{
    public static class IUserExtensions
    {
        public static TicketAssignee ToAssignee(this IUser user)
        {
            return new TicketAssignee
                   {
                       Type      = user.Type switch
                                   {
                                       UserType.Customer   => TicketAssigneeType.CustomerUser,
                                       UserType.Workgroup  => TicketAssigneeType.WorkGroup,
                                       _                   => TicketAssigneeType.NoAssignee
                                   },

                       Id        = user.Id,
                       FirstName = user.FirstName,
                       LastName  = user.LastName,
                       Email     = user.Email,
                       Name      = user.Name
                   };
        }

        public static TicketCreator ToCreator(this User user)
        {
			return new TicketCreator
			{
				Id = user.Id,
				Type = user.Type,
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Mobile = user.Mobile,
				Company = user.Company,
				Initials = user.Initials

			};
		}
      public static async Task<IDictionary<Guid, IUser>> GetAssignees(this IUserService userService, Guid siteId, IEnumerable<Assignable> tickets)
        {
            var assigneeIds = tickets.Where( t=> t.AssigneeId.HasValue).Select( t=> t.AssigneeUserId() ).Distinct();
            
            return (await userService.GetUsers(siteId, assigneeIds, UserType.All)).Distinct(new UserComparer()).ToDictionary(k=> k.Id, v=> v);
        }
    }
}
