using System;

namespace MobileXL.Models
{
    public class CustomerUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? AvatarId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Initials { get; set; }
        public string Auth0UserId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Company { get; set; }
        public Guid CustomerId { get; set; }
        public UserStatus Status { get; set; }
    }
}
