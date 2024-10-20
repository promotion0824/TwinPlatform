using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class PersonDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public SitePersonType Type { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Company { get; set; }
        public string ContactNumber { get; set; }
        public UserStatus Status { get; set; }
        public RoleDto Role { get; set; }
        public List<SiteSimpleDto> Sites { get; set; } = new List<SiteSimpleDto>();
        public List<PortfolioSimpleDto> Portfolios { get; set; } = new List<PortfolioSimpleDto>();

        public static PersonDetailDto Map(Reporter model)
        {
            return new PersonDetailDto
            {
                Id = model.Id,
                Name = model.Name,
                Email = model.Email,
                Company = model.Company,
                ContactNumber = model.Phone,
                Type = SitePersonType.Reporter
            };
        }

        public static PersonDetailDto Map(User model)
        {
            return new PersonDetailDto
            {
                Id = model.Id,
                Name = $"{model.FirstName} {model.LastName}",
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                ContactNumber = model.Mobile,
                Status = model.Status,
                Company = model.Company,
                Role = RoleDto.Map(model.Role),
                Type = SitePersonType.CustomerUser,
                CreatedDate = model.CreatedDate
            };
        }
        public static List<PersonDetailDto> Map(IEnumerable<Reporter> models)
        {
            return models?.Select(Map).ToList();
        }

        public static List<PersonDetailDto> Map(IEnumerable<User> models)
        {
            return models?.Select(Map).ToList();
        }
        
    }

    public class SiteSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PortfolioSimpleDto Portfolio { get; set; }
    }

    public class PortfolioSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
