using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class UserSimpleDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public string Email { get; set; }

        public static UserSimpleDto Map(IUser model)
        {
            return new UserSimpleDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Initials = model.Initials,
                Email = model.Email,
            };
        }

        public static List<UserSimpleDto> Map(IEnumerable<User> models)
        {
            return models?.Select(Map).ToList();
        }
    }
}
