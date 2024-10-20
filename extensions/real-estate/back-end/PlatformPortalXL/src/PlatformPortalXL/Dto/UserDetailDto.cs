using System;
using PlatformPortalXL.Models;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Initials { get; set; }
        public string Auth0UserId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Company { get; set; }
        public object Preferences { get; set; }
        public CustomerDto Customer { get; set; }

        public static UserDetailDto Map(User model)
        {
            return new UserDetailDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedDate = model.CreatedDate,
                Initials = model.Initials,
                Auth0UserId = model.Auth0UserId,
                Email = model.Email,
                Mobile = model.Mobile,
                Company = model.Company,
                Preferences = new { }
            };
        }
    }
}
