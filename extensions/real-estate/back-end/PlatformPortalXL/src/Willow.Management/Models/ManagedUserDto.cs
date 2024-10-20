using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Platform.Users;

namespace Willow.Management
{
    public class ManagedUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Company { get; set; }
        public string ContactNumber { get; set; }
        public bool IsCustomerAdmin { get; set; }
        public UserStatus Status { get; set; }
        public bool CanEdit { get; set; } = true;

        public List<ManagedPortfolioDto> Portfolios { get; set; } = new List<ManagedPortfolioDto>();
        public static ManagedUserDto Map(User model)
        {
            return new ManagedUserDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Initials = model.Initials,
                Email = model.Email,
                CreatedDate = model.CreatedDate,
                Company = model.Company,
                ContactNumber = model.Mobile,
                Status = model.Status
            };
        }

        public static List<ManagedUserDto> Map(IEnumerable<User> models)
        {
            return models?.Select(model => Map(model)).ToList();
        }
    }
}
