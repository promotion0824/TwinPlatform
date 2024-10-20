using System;

namespace AdminPortalXL.Models.Directory
{
    public class Supervisor
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Initials { get; set; }
        public string Auth0UserId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}
