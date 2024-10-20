using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Domain;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Infrastructure.Exceptions;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace DirectoryCore.Services
{
    public interface ICustomerUsersService
    {
        Task<User> GetCustomerUserByEmail(string email);
        Task<IList<User>> GetCustomerUsers(Guid customerId);
        Task<User> GetCustomerUser(Guid customerId, Guid userId);
        Task<User> CreateCustomerUser(
            Guid customerId,
            CreateCustomerUserRequest createCustomerUserRequest,
            string language
        );
        Task UpdateCustomerUser(
            Guid customerId,
            Guid userId,
            UpdateCustomerUserRequest updateCustomerUserRequest
        );
        Task InactivateCustomerUser(Guid customerId, Guid userId);
        Task SendActivationEmail(Guid customerId, Guid userId, string language);
    }

    public class CustomerUsersService : ICustomerUsersService
    {
        private static double EmailTokenExpiryHours = 48;

        private readonly DirectoryDbContext _directoryContext;
        private readonly SignUpOption _signUpOption;

        private readonly IDateTimeService _dateTime;
        private readonly IAuth0ManagementService _auth0ManagementService;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public CustomerUsersService(
            DirectoryDbContext directoryContext,
            IOptionsMonitor<SignUpOption> signUpOption,
            IDateTimeService dateTime,
            IAuth0ManagementService auth0ManagementService,
            INotificationService notificationService,
            ILogger<UsersService> logger
        )
        {
            _directoryContext = directoryContext;
            _signUpOption = signUpOption.CurrentValue;
            _dateTime = dateTime;
            _auth0ManagementService = auth0ManagementService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<User> GetCustomerUserByEmail(string email)
        {
            var entity = await _directoryContext
                .Users.Include(x => x.Preferences)
                .FirstOrDefaultAsync(x => x.Email == email);
            if (entity == null)
            {
                return null;
            }
            return UserEntity.MapTo(entity);
        }

        public async Task<IList<User>> GetCustomerUsers(Guid customerId)
        {
            var query = _directoryContext
                .Users.Include(x => x.Preferences)
                .Where(x => x.CustomerId == customerId);

            var users = await query.ToListAsync();
            return UserEntity.MapTo(users);
        }

        public async Task<User> GetCustomerUser(Guid customerId, Guid userId)
        {
            var user = await _directoryContext
                .Users.Include(x => x.Preferences)
                .FirstOrDefaultAsync(x => x.Id == userId && x.CustomerId == customerId);
            if (user == null)
            {
                throw new ResourceNotFoundException("customerUser", userId);
            }
            return UserEntity.MapTo(user);
        }

        public async Task<User> CreateCustomerUser(
            Guid customerId,
            CreateCustomerUserRequest createCustomerUserRequest,
            string language
        )
        {
            var customer = await _directoryContext.Customers.FirstOrDefaultAsync(
                c => c.Id == customerId
            );
            if (customer == null)
            {
                throw new ResourceNotFoundException("customers", customerId);
            }

            var user = await _directoryContext.Users.FirstOrDefaultAsync(
                u => u.Email == createCustomerUserRequest.Email
            );
            var token = string.Empty;
            if (user != null)
            {
                throw new BadRequestException(
                    $"Failed to create customer user. The customer user of {createCustomerUserRequest.Email} exists."
                );
            }
            else
            {
                token = GenerateEmailConfirmationToken();

                var userStatus = createCustomerUserRequest.UseB2C
                    ? UserStatus.Active
                    : UserStatus.Pending;

                user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = createCustomerUserRequest.Email,
                    FirstName = createCustomerUserRequest.FirstName,
                    LastName = createCustomerUserRequest.LastName,
                    CreatedDate = DateTime.UtcNow,
                    Auth0UserId = string.Empty,
                    Initials = string.Empty,
                    Mobile = createCustomerUserRequest.Mobile,
                    EmailConfirmationToken = token,
                    EmailConfirmationTokenExpiry = _dateTime.UtcNow.AddHours(EmailTokenExpiryHours),
                    CustomerId = customer.Id,
                    Status = userStatus,
                    Company = createCustomerUserRequest.Company
                };

                _directoryContext.Users.Add(user);
            }

            try
            {
                await _directoryContext.SaveChangesAsync();

                if (!createCustomerUserRequest.UseB2C && !string.IsNullOrEmpty(token))
                {
                    var parameters = new
                    {
                        UserInitializationUrl = $"{_signUpOption.CustomerUserInitializationUrl}&t={System.Web.HttpUtility.UrlEncode(token)}"
                    };
                    await _notificationService.SendNotificationAsync(
                        new Willow.Notifications.Models.Notification
                        {
                            CorrelationId = Guid.NewGuid(),
                            CommunicationType = CommunicationType.Email,
                            CustomerId = customer.Id,
                            Data = parameters.ToDictionary(),
                            Tags = null,
                            TemplateName = "InitializeUser",
                            UserId = user.Id,
                            Locale = language
                        }
                    );
                }

                return UserEntity.MapTo(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Customer user was not created. Reason: {Message}",
                    ex.Message
                );
                throw new BadRequestException("Failed to create customer user");
            }
        }

        public async Task UpdateCustomerUser(
            Guid customerId,
            Guid userId,
            UpdateCustomerUserRequest updateCustomerUserRequest
        )
        {
            var existingCustomer = await _directoryContext
                .Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (existingCustomer == null)
            {
                throw new ResourceNotFoundException("customers", customerId);
            }

            var existingUser = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == userId);

            var existingCustomerUser = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == userId && c.CustomerId == customerId);

            if (existingUser == null || existingCustomerUser == null)
            {
                throw new ResourceNotFoundException("users", userId);
            }

            existingUser.FirstName = updateCustomerUserRequest.FirstName;
            existingUser.LastName = updateCustomerUserRequest.LastName;
            existingUser.Mobile = updateCustomerUserRequest.Mobile;
            existingUser.Company = updateCustomerUserRequest.Company;

            _directoryContext.Users.Update(existingUser);
            await _directoryContext.SaveChangesAsync();
        }

        public async Task InactivateCustomerUser(Guid customerId, Guid userId)
        {
            var existingUser = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == userId);
            if (existingUser == null)
            {
                throw new ResourceNotFoundException("users", userId);
            }

            if (!string.IsNullOrEmpty(existingUser.Auth0UserId))
            {
                await _auth0ManagementService.InactivateUser(existingUser.Auth0UserId);
            }
            existingUser.Status = UserStatus.Inactive;
            existingUser.LastName = WellKnownUsers.DeletedUser.InactiveName(existingUser.LastName);
            _directoryContext.Users.Update(existingUser);

            await _directoryContext.SaveChangesAsync();
        }

        public async Task SendActivationEmail(Guid customerId, Guid userId, string language)
        {
            var customer = await _directoryContext.Customers.FirstOrDefaultAsync(
                c => c.Id == customerId
            );
            if (customer == null)
            {
                throw new ResourceNotFoundException("customers", customerId);
            }

            var user = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == userId);
            if (user == null)
            {
                throw new ResourceNotFoundException("user", userId);
            }

            if (user.Status == UserStatus.Pending)
            {
                var token = GenerateEmailConfirmationToken();
                user.EmailConfirmationToken = token;
                user.EmailConfirmationTokenExpiry = _dateTime.UtcNow.AddHours(
                    EmailTokenExpiryHours
                );
                await _directoryContext.SaveChangesAsync();
                var parameters = new
                {
                    UserInitializationUrl = $"{_signUpOption.CustomerUserInitializationUrl}&t={System.Web.HttpUtility.UrlEncode(token)}"
                };
                await _notificationService.SendNotificationAsync(
                    new Willow.Notifications.Models.Notification
                    {
                        CorrelationId = Guid.NewGuid(),
                        CommunicationType = CommunicationType.Email,
                        CustomerId = customer.Id,
                        Data = parameters.ToDictionary(),
                        Tags = null,
                        TemplateName = "InitializeUser",
                        UserId = user.Id,
                        Locale = language
                    }
                );
            }
            else
            {
                throw new BadRequestException("User has been activated or deleted");
            }
        }

        private static string GenerateEmailConfirmationToken()
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }
    }
}
