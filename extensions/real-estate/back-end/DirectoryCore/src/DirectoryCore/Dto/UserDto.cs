using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid? AvatarId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Initials { get; set; }

        public string Auth0UserId { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public UserStatus Status { get; set; }

        public Guid CustomerId { get; set; }

        public RoleDto Role { get; set; }

        public string Company { get; set; }

        public string Language { get; set; }

        public static UserDto MapFrom(User user)
        {
            if (user == null)
            {
                return null;
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarId = user.AvatarId,
                CreatedDate = user.CreatedDate,
                Initials = user.Initials,
                Auth0UserId = user.Auth0UserId,
                Email = user.Email,
                Mobile = user.Mobile,
                Status = user.Status,
                CustomerId = user.CustomerId,
                Role = RoleDto.MapFrom(user.Role),
                Company = user.Company,
                Language = user.Language
            };
            return userDto;
        }

        public static IList<UserDto> MapFrom(IEnumerable<User> users)
        {
            return users.Select(MapFrom).ToList();
        }
    }
}
