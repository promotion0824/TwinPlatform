using System;
using System.ComponentModel.DataAnnotations;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Entities
{
    public class SupervisorEntity
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
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

        public static Supervisor MapTo(SupervisorEntity supervisor)
        {
            if (supervisor == null)
            {
                return null;
            }

            return new Supervisor
            {
                Id = supervisor.Id,
                FirstName = supervisor.FirstName,
                LastName = supervisor.LastName,
                AvatarId = supervisor.AvatarId,
                CreatedDate = supervisor.CreatedDate,
                Initials = supervisor.Initials,
                Auth0UserId = supervisor.Auth0UserId,
                Email = supervisor.Email,
                EmailConfirmed = supervisor.EmailConfirmed,
                EmailConfirmationToken = supervisor.EmailConfirmationToken,
                EmailConfirmationTokenExpiry = supervisor.EmailConfirmationTokenExpiry,
                Mobile = supervisor.Mobile,
                Status = supervisor.Status
            };
        }
    }
}
