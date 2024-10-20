using PlatformPortalXL.Features.Management;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;
using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class PersonDto
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
        public RoleDto Role { get; set; }
        public UserStatus Status { get; set; }

        public static PersonDto Map(Reporter model)
        {
            return new PersonDto
            {
                Id = model.Id,
                Name = model.Name,
                Email = model.Email,
                Company = model.Company,
                ContactNumber = model.Phone,
                Type = SitePersonType.Reporter
            };
        }

        public static PersonDto Map(User model)
        {
            return new PersonDto
            {
                Id = model.Id,
                Name = $"{model.FirstName} {model.LastName}",
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                ContactNumber = model.Mobile,
                Company = model.Company,
                Role = RoleDto.Map(model.Role),
                Type = SitePersonType.CustomerUser,
                CreatedDate = model.CreatedDate,
                Status = model.Status
            };
        }

        public static List<PersonDto> Map(IEnumerable<Reporter> models)
        {
            return models?.Select(Map).ToList();
        }

        public static List<PersonDto> Map(IEnumerable<User> models)
        {
            return models?.Select(Map).ToList();
        }   
    }
}
