using System;
using System.Globalization;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Domain;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Infrastructure.Exceptions;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace DirectoryCore.Services
{
    public interface ISupervisorsService
    {
        Task<Supervisor> GetSupervisorByAuth0Id(string auth0Id);
        Task<Supervisor> GetSupervisorByEmailAddress(string emailAddress);
        Task<Supervisor> GetSupervisor(Guid supervisorId);
        Task SendResetPasswordEmail(string supervisorEmail, string language);
        Task<string> GetSupervisorEmailByToken(string emailToken);
        Task ChangeSupervisorPassword(string supervisorEmail, string emailToken, string password);
        Task ConfirmEmailToken(Guid supervisorId, string emailToken);
    }

    public class SupervisorsService : ISupervisorsService
    {
        private const double EmailTokenExpiryHours = 48;

        private readonly IDateTimeService _dateTime;
        private readonly SignUpOption _signUpOption;
        private readonly INotificationService _notificationService;
        private readonly DirectoryDbContext _directoryContext;
        private readonly IAuth0ManagementService _auth0ManagementService;

        public SupervisorsService(
            DirectoryDbContext directoryContext,
            IDateTimeService dateTime,
            INotificationService notificationService,
            IAuth0ManagementService auth0ManagementService,
            IOptionsMonitor<SignUpOption> signUpOption
        )
        {
            _auth0ManagementService = auth0ManagementService;
            _signUpOption = signUpOption.CurrentValue;
            _directoryContext = directoryContext;
            _notificationService = notificationService;
            _dateTime = dateTime;
        }

        public async Task<Supervisor> GetSupervisorByAuth0Id(string auth0Id)
        {
            var supervisor = await _directoryContext.Supervisors.FirstOrDefaultAsync(
                s => s.Auth0UserId == auth0Id
            );

            return SupervisorEntity.MapTo(supervisor);
        }

        public async Task<Supervisor> GetSupervisorByEmailAddress(string emailAddress)
        {
            var supervisor = await _directoryContext.Supervisors.FirstOrDefaultAsync(
                s =>
                    s.Email == emailAddress
                    && s.Status != UserStatus.Inactive
                    && s.Status != UserStatus.Deleted
            );

            return SupervisorEntity.MapTo(supervisor);
        }

        public async Task<Supervisor> GetSupervisor(Guid supervisorId)
        {
            var supervisor = await _directoryContext.Supervisors.FirstOrDefaultAsync(
                c => c.Id == supervisorId
            );

            return SupervisorEntity.MapTo(supervisor);
        }

        public async Task SendResetPasswordEmail(string supervisorEmail, string language)
        {
            var supervisor = await _directoryContext
                .Supervisors.AsTracking()
                .FirstOrDefaultAsync(c => c.Email == supervisorEmail);

            if (supervisor == null)
            {
                throw new ResourceNotFoundException("supervisor", supervisorEmail);
            }
            if (string.IsNullOrEmpty(supervisor.Auth0UserId))
            {
                throw new BadRequestException("The supervisor has not been initialized.");
            }
            if (supervisor.Status != UserStatus.Active)
            {
                throw new BadRequestException("The supervisor is not active.");
            }

            var token = GenerateEmailConfirmationToken();
            supervisor.EmailConfirmationToken = token;
            supervisor.EmailConfirmationTokenExpiry = _dateTime.UtcNow.AddHours(
                EmailTokenExpiryHours
            );
            supervisor.EmailConfirmed = false;

            _directoryContext.Supervisors.Update(supervisor);
            await _directoryContext.SaveChangesAsync();

            var parameters = new
            {
                UserInitializationUrl = $"{_signUpOption.AdminResetPasswordUrl}&t={System.Web.HttpUtility.UrlEncode(token)}",
                //UserName = "{supervisor.FirstName} {supervisor.LastNam}",
                //CustomerName = "Willow"
            };

            await _notificationService.SendNotificationAsync(
                new Willow.Notifications.Models.Notification
                {
                    CorrelationId = Guid.NewGuid(),
                    CommunicationType = CommunicationType.Email,
                    CustomerId = Guid.Empty,
                    Data = parameters.ToDictionary(),
                    Tags = null,
                    TemplateName = "ResetPassword",
                    UserId = supervisor.Id,
                    Locale = language,
                    UserType = UserTypeNames.Supervisor
                }
            );
        }

        public async Task<string> GetSupervisorEmailByToken(string emailToken)
        {
            var supervisor = await _directoryContext
                .Supervisors.AsTracking()
                .FirstOrDefaultAsync(c => c.EmailConfirmationToken == emailToken);

            if (supervisor == null)
            {
                throw new ResourceNotFoundException("token", emailToken);
            }
            if (supervisor.EmailConfirmed)
            {
                throw new BadRequestException("The email token has been used");
            }
            if (_dateTime.UtcNow > supervisor.EmailConfirmationTokenExpiry)
            {
                throw new BadRequestException("The email token expires");
            }

            return supervisor.Email;
        }

        public async Task ChangeSupervisorPassword(
            string supervisorEmail,
            string emailToken,
            string password
        )
        {
            var existingSupervisor = await _directoryContext.Supervisors.FirstOrDefaultAsync(
                c => c.Email == supervisorEmail
            );
            if (existingSupervisor == null)
            {
                throw new ResourceNotFoundException("supervisor", supervisorEmail);
            }

            await ConfirmEmailToken(existingSupervisor.Id, emailToken);

            if (!string.IsNullOrEmpty(existingSupervisor.Auth0UserId))
            {
                await _auth0ManagementService.ChangeUserPassword(
                    existingSupervisor.Auth0UserId,
                    password
                );
            }
        }

        public async Task ConfirmEmailToken(Guid supervisorId, string emailToken)
        {
            var supervisor = await _directoryContext
                .Supervisors.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == supervisorId);

            if (supervisor == null)
            {
                throw new ResourceNotFoundException("supervisor", supervisorId);
            }
            if (supervisor.EmailConfirmationToken != emailToken)
            {
                throw new BadRequestException("The email token does not match");
            }
            if (supervisor.EmailConfirmed)
            {
                throw new BadRequestException("The email token has been used");
            }
            if (_dateTime.UtcNow > supervisor.EmailConfirmationTokenExpiry)
            {
                throw new BadRequestException("The email token expires");
            }

            supervisor.EmailConfirmed = true;
            await _directoryContext.SaveChangesAsync();
        }

        private static string GenerateEmailConfirmationToken()
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }
    }
}
