using System;
using AdminPortalXL.Models.Directory;

namespace AdminPortalXL.Dto
{
    public class SupervisorDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Initials { get; set; }
        public string Auth0UserId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public static SupervisorDto Map(Supervisor model)
        {
            if (model == null)
            {
                return null;
            }

            return new SupervisorDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedDate = model.CreatedDate,
                Initials = model.Initials,
                Auth0UserId = model.Auth0UserId,
                Email = model.Email,
                Mobile = model.Mobile,
            };
        }
    }
}
