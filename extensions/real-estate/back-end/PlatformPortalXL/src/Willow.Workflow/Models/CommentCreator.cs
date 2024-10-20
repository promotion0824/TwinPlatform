using System;

using Willow.Platform.Users;

namespace Willow.Workflow
{
    public class CommentCreator
    {
        public CommentCreatorType Type { get; set; }
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public static CommentCreator FromCustomerUser(User customerUser)
        {
            return new CommentCreator
            {
                Type = CommentCreatorType.CustomerUser,
                Id = customerUser.Id,
                FirstName = customerUser.FirstName,
                LastName = customerUser.LastName,
                Email = customerUser.Email
            };
        }
    }
}
