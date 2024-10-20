using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Features.Management;

using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class RequestorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public SitePersonType Type { get; set; }

        public static RequestorDto MapFromModel(Reporter model)
        {
            return new RequestorDto
            {
                Id = model.Id,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Company = model.Company,
                Type = SitePersonType.Reporter
            };
        }

        public static RequestorDto MapFromModel(User model)
        {
            return new RequestorDto
            {
                Id = model.Id,
                Name = $"{model.FirstName} {model.LastName}",
                Phone = model.Mobile,
                Email = model.Email,
                Company = model.Company,
                Type = SitePersonType.CustomerUser
            };
        }

        public static List<RequestorDto> MapFromModels(List<Reporter> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

        public static List<RequestorDto> MapFromModels(List<User> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
