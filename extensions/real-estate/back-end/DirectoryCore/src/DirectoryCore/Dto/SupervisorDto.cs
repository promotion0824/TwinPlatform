using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Dto
{
    public class SupervisorDto
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

        public static SupervisorDto MapFrom(Supervisor supervisor)
        {
            if (supervisor == null)
            {
                return null;
            }

            var supervisorDto = new SupervisorDto
            {
                Id = supervisor.Id,
                FirstName = supervisor.FirstName,
                LastName = supervisor.LastName,
                AvatarId = supervisor.AvatarId,
                CreatedDate = supervisor.CreatedDate,
                Initials = supervisor.Initials,
                Auth0UserId = supervisor.Auth0UserId,
                Email = supervisor.Email,
                Mobile = supervisor.Mobile,
                Status = supervisor.Status
            };
            return supervisorDto;
        }

        public static IList<SupervisorDto> MapFrom(IEnumerable<Supervisor> supervisors) =>
            supervisors.Select(MapFrom).ToList();
    }
}
