using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Entities
{
    [Table("Users")]
    public class UserEntity
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string EmailConfirmationToken { get; set; }

        public DateTime EmailConfirmationTokenExpiry { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string LastName { get; set; }

        public Guid? AvatarId { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(20)]
        public string Initials { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string Auth0UserId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Mobile { get; set; }

        public UserStatus Status { get; set; }

        [MaxLength(100)]
        public string Company { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        public CustomerUserPreferencesEntity Preferences { get; set; }

        public CustomerUserTimeSeriesEntity TimeSeries { get; set; }

        public static User MapTo(UserEntity user)
        {
            if (user == null)
            {
                return null;
            }

            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarId = user.AvatarId,
                CreatedDate = user.CreatedDate,
                Initials = user.Initials,
                Auth0UserId = user.Auth0UserId,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                EmailConfirmationToken = user.EmailConfirmationToken,
                EmailConfirmationTokenExpiry = user.EmailConfirmationTokenExpiry,
                Mobile = user.Mobile,
                Status = user.Status,
                CustomerId = user.CustomerId,
                Company = user.Company,
                Language = user.Preferences?.Language
            };
        }

        public static List<User> MapTo(List<UserEntity> users)
        {
            return users?.Select(MapTo).ToList() ?? new List<User>();
        }
    }
}
