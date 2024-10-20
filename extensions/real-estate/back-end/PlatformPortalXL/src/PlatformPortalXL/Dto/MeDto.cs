using System;
using System.Collections.Generic;
using PlatformPortalXL.Auth;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class MeDto
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
        public bool IsCustomerAdmin { get; set; }
        public bool ShowAdminMenu { get; set; }
        public bool ShowPortfolioTab { get; set; }
        public bool ShowRulingEngineMenu { get; set; }
        public List<PortfolioDto> Portfolios { get; set; }
        public IReadOnlyList<string> Policies { get; set; }

        public static MeDto Map(User model)
        {
            return new MeDto
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
