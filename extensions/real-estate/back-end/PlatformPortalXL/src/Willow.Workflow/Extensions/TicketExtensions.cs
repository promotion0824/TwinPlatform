using System;

using Willow.Platform.Users;

namespace Willow.Workflow
{
    public static class TicketExtensions
    {
        public static UserType ToUserType(this TicketAssigneeType type)
        {
            return type switch
            {
                TicketAssigneeType.CustomerUser => UserType.Customer,
                TicketAssigneeType.WorkGroup => UserType.Workgroup,
                TicketAssigneeType.NoAssignee => UserType.Unknown,
                _ => UserType.Unknown
            };
        }

        public static (Guid UserId, UserType UserType) AssigneeUserId(this Assignable ticket)
        {
            return (ticket.AssigneeId.Value, ticket.AssigneeType.ToUserType());
        }

        public static (Guid, UserType) CommentUserId(this Comment comment)
        {
            var type = comment.CreatorType switch
                                           {
                                               CommentCreatorType.CustomerUser => UserType.Customer,
                                               _                               => UserType.Unknown
                                           };

            return (comment.CreatorId, type);
        }
    }
}
