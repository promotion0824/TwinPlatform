using System;
using DirectoryCore.Enums;

namespace DirectoryCore.Domain
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string EmailConfirmationToken { get; set; }

        public DateTime EmailConfirmationTokenExpiry { get; set; }

        public bool EmailConfirmed { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid? AvatarId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Initials { get; set; }

        public string Auth0UserId { get; set; }

        public string Mobile { get; set; }

        public UserStatus Status { get; set; }

        public Guid CustomerId { get; set; }

        public Role Role { get; set; }

        public string Company { get; set; }
        public string Language { get; set; }
    }
}
