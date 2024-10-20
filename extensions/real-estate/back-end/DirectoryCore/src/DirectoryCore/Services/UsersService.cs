using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Data;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace DirectoryCore.Services
{
    public interface IUsersService
    {
        Task<User> GetUser(Guid userId, Domain.UserType userType);
        Task<User> GetUserByAuth0Id(string auth0Id);
        Task<User> GetUserByEmailAddress(string emailAddress);
        Task InitializeUser(string userEmail, InitializeUserRequest initializeUserRequest);
        Task ConfirmEmailToken(Guid userId, string emailToken);
        Task<string> GetUserEmailByToken(string emailToken);
        Task SendResetPasswordEmail(string userEmail, string language);
        Task ChangePassword(string userEmail, string emailToken, string password);
        Task CreateOrUpdateConnectorAccount(
            Guid customerId,
            Guid siteId,
            Guid connectorId,
            string password
        );
        Task<IList<User>> GetSiteUsers(Guid siteId);
        Task<IList<User>> GetPortfolioUsers(Guid portfolioId);
        Task<IList<Role>> GetRoles();
        Task<CustomerUserPreferences> GetCustomerUserPreferences(Guid customerUserId);
        Task CreateOrUpdateCustomerUserPreference(
            Guid customerUserId,
            CustomerUserPreferencesRequest customerUserPreferencesRequest
        );
        Task<CustomerUserTimeSeriesDto> GetCustomerUserTimeSeries(Guid customerUserId);
        Task CreateOrUpdateCustomerUserTimeSeries(
            Guid customerUserId,
            CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest
        );
        Task<List<User>> GetFullNamesByUserIdsAync(List<Guid> userIds);
        Task<List<User>> GetUsersProfilesAsync(GetUsersProfilesRequest getUserProfileRequest);

        Task<UserDetailsDto> GetUserDetailsAsync(Guid userId);
    }

    public class UsersService : IUsersService
    {
        private const double EmailTokenExpiryHours = 48;
        private const string DefaultLanguage = "en";

        private readonly IDateTimeService _dateTime;
        private readonly DirectoryDbContext _directoryContext;
        private readonly SignUpOption _signUpOption;
        private readonly IAuth0ManagementService _auth0ManagementService;
        private readonly INotificationService _notificationService;
        private readonly ISitesService _sitesService;
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IAppCache _appCache;

        public UsersService(
            DirectoryDbContext directoryContext,
            INotificationService notificationService,
            IDateTimeService dateTime,
            IOptionsMonitor<SignUpOption> signUpOption,
            IAuth0ManagementService auth0ManagementService,
            ISitesService sitesService,
            IImagePathHelper imagePathHelper,
            IAppCache appCache
        )
        {
            _directoryContext = directoryContext;
            _signUpOption = signUpOption.CurrentValue;
            _auth0ManagementService = auth0ManagementService;
            _notificationService = notificationService;
            _dateTime = dateTime;
            _sitesService = sitesService;
            _imagePathHelper = imagePathHelper;
            _appCache = appCache;
        }

        public async Task<User> GetUser(Guid userId, Domain.UserType userType)
        {
            if (userType.HasFlag(Domain.UserType.Customer))
            {
                var query =
                    from u in _directoryContext.Users
                    join p in _directoryContext.CustomerUserPreferences
                        on u.Id equals p.CustomerUserId
                        into pp
                    from prefs in pp.DefaultIfEmpty()
                    where u.Id == userId
                    select new User
                    {
                        Id = u.Id,
                        CustomerId = u.CustomerId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Mobile = u.Mobile,
                        Company = u.Company,
                        CreatedDate = u.CreatedDate,
                        EmailConfirmed = u.EmailConfirmed,
                        EmailConfirmationToken = u.EmailConfirmationToken,
                        EmailConfirmationTokenExpiry = u.EmailConfirmationTokenExpiry,
                        AvatarId = u.AvatarId,
                        Initials = u.Initials,
                        Auth0UserId = u.Auth0UserId,
                        Status = u.Status,
                        Language = prefs == null ? DefaultLanguage : prefs.Language,
                    };

                var user = await query.SingleOrDefaultAsync();

                if (user != null)
                {
                    return user;
                }
            }
            if (userType.HasFlag(Domain.UserType.Supervisor))
            {
                var supervisor = await _directoryContext.Supervisors.SingleOrDefaultAsync(
                    c => c.Id == userId && c.Status == UserStatus.Active
                );

                if (supervisor != null)
                {
                    return new User
                    {
                        Id = userId,
                        Email = supervisor.Email,
                        FirstName = supervisor.FirstName,
                        LastName = supervisor.LastName,
                        Mobile = supervisor.Mobile,
                        Status = supervisor.Status,
                        Language = DefaultLanguage
                    };
                }
            }

            return null;
        }

        public async Task<User> GetUserByAuth0Id(string auth0Id)
        {
            var user = await _directoryContext.Users.FirstOrDefaultAsync(
                u => u.Auth0UserId == auth0Id
            );

            return UserEntity.MapTo(user);
        }

        public async Task<User> GetUserByEmailAddress(string emailAddress)
        {
            var userData = await _appCache.GetOrAddAsync(
                $"get-user-by-email-{emailAddress}",
                async cache =>
                {
                    cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(15));

                    var user = await _directoryContext.Users.FirstOrDefaultAsync(
                        u =>
                            u.Email == emailAddress
                            && u.Status != UserStatus.Inactive
                            && u.Status != UserStatus.Deleted
                    );

                    return UserEntity.MapTo(user);
                }
            );

            return userData;
        }

        public async Task InitializeUser(
            string userEmail,
            InitializeUserRequest initializeUserRequest
        )
        {
            var existingUser = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Email == userEmail);
            if (existingUser == null)
            {
                throw new ResourceNotFoundException("user", userEmail);
            }

            await ConfirmEmailToken(existingUser.Id, initializeUserRequest.EmailToken);

            if (string.IsNullOrEmpty(existingUser.Auth0UserId))
            {
                var auth0UserId = await _auth0ManagementService.CreateUser(
                    existingUser.Id,
                    existingUser.Email,
                    existingUser.FirstName,
                    existingUser.LastName,
                    initializeUserRequest.Password,
                    UserTypeNames.CustomerUser
                );

                existingUser.Auth0UserId = auth0UserId;
                existingUser.Status = UserStatus.Active;

                await _directoryContext.SaveChangesAsync();
            }
        }

        public async Task ConfirmEmailToken(Guid userId, string emailToken)
        {
            var user = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ResourceNotFoundException("user", userId);
            }
            if (user.EmailConfirmationToken != emailToken)
            {
                throw new BadRequestException("The email token does not match");
            }
            if (user.EmailConfirmed)
            {
                throw new BadRequestException("The email token has been used");
            }
            if (_dateTime.UtcNow > user.EmailConfirmationTokenExpiry)
            {
                throw new BadRequestException("The email token expires");
            }

            user.EmailConfirmed = true;
            await _directoryContext.SaveChangesAsync();
        }

        public async Task<string> GetUserEmailByToken(string emailToken)
        {
            var user = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.EmailConfirmationToken == emailToken);

            if (user == null)
            {
                throw new ResourceNotFoundException("token", emailToken);
            }
            if (user.EmailConfirmed)
            {
                throw new BadRequestException("The email token has been used");
            }
            if (_dateTime.UtcNow > user.EmailConfirmationTokenExpiry)
            {
                throw new BadRequestException("The email token expires");
            }

            return user.Email;
        }

        public async Task SendResetPasswordEmail(string userEmail, string language)
        {
            var user = await _directoryContext
                .Users.AsTracking()
                .FirstOrDefaultAsync(c => c.Email == userEmail);
            if (user == null)
            {
                throw new ResourceNotFoundException("user", userEmail);
            }
            if (string.IsNullOrEmpty(user.Auth0UserId))
            {
                throw new BadRequestException("The user has not been initialized.");
            }
            if (user.Status != UserStatus.Active)
            {
                throw new BadRequestException("The user is not active.");
            }

            var token = GenerateEmailConfirmationToken();
            user.EmailConfirmationToken = token;
            user.EmailConfirmationTokenExpiry = _dateTime.UtcNow.AddHours(EmailTokenExpiryHours);
            user.EmailConfirmed = false;

            _directoryContext.Users.Update(user);
            await _directoryContext.SaveChangesAsync();

            var customer = await _directoryContext.Customers.FirstOrDefaultAsync(
                c => c.Id == user.CustomerId
            );

            var parameters = new
            {
                UserInitializationUrl = $"{_signUpOption.CustomerUserResetPasswordUrl}&t={System.Web.HttpUtility.UrlEncode(token)}",
            };
            await _notificationService.SendNotificationAsync(
                new Willow.Notifications.Models.Notification
                {
                    CorrelationId = Guid.NewGuid(),
                    CommunicationType = CommunicationType.Email,
                    CustomerId = customer.Id,
                    Data = parameters.ToDictionary(),
                    Tags = null,
                    TemplateName = "ResetPassword",
                    UserId = user.Id,
                    Locale = language
                }
            );
        }

        private static string GenerateEmailConfirmationToken()
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }

        public async Task ChangePassword(string userEmail, string emailToken, string password)
        {
            var existingUser = await _directoryContext.Users.FirstOrDefaultAsync(
                c => c.Email == userEmail
            );
            if (existingUser == null)
            {
                throw new ResourceNotFoundException("user", userEmail);
            }

            await ConfirmEmailToken(existingUser.Id, emailToken);

            if (!string.IsNullOrEmpty(existingUser.Auth0UserId))
            {
                await _auth0ManagementService.ChangeUserPassword(
                    existingUser.Auth0UserId,
                    password
                );
            }
        }

        public async Task CreateOrUpdateConnectorAccount(
            Guid customerId,
            Guid siteId,
            Guid connectorId,
            string password
        )
        {
            var email = GetConnectorAccountEmail(connectorId);
            var connectorAccountUserId = await _auth0ManagementService.GetAuth0UserId(email);
            if (string.IsNullOrEmpty(connectorAccountUserId))
            {
                var customer = await _directoryContext.Customers.FirstOrDefaultAsync(
                    c => c.Id == customerId
                );
                var site = await _sitesService.GetSite(siteId);

                if (customer == null)
                {
                    throw new ResourceNotFoundException("customer", customerId);
                }
                if (site == null)
                {
                    throw new ResourceNotFoundException("site", siteId);
                }

                await _auth0ManagementService.CreateUser(
                    connectorId,
                    email,
                    site.Name,
                    customer.Name,
                    password,
                    UserTypeNames.Connector
                );
            }
            else
            {
                await _auth0ManagementService.ChangeUserPassword(connectorAccountUserId, password);
            }
        }

        public async Task<IList<User>> GetSiteUsers(Guid siteId)
        {
            var query =
                from a in _directoryContext.Assignments
                join u in _directoryContext.Users on a.PrincipalId equals u.Id
                join r in _directoryContext.Roles on a.RoleId equals r.Id
                where a.ResourceType == RoleResourceType.Site && a.ResourceId == siteId
                select new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    Company = u.Company,
                    CreatedDate = u.CreatedDate,
                    Role = new Role { Id = r.Id, Name = r.Name },
                    Status = u.Status
                };

            return await query.ToListAsync();
        }

        private static string GetConnectorAccountEmail(Guid connectorId)
        {
            return $"{connectorId:N}@connector.willowinc.com";
        }

        public async Task<IList<Role>> GetRoles()
        {
            var roles = await _directoryContext.Roles.ToListAsync();
            return RoleEntity.MapTo(roles);
        }

        public async Task<IList<User>> GetPortfolioUsers(Guid portfolioId)
        {
            var query =
                from a in _directoryContext.Assignments
                join u in _directoryContext.Users on a.PrincipalId equals u.Id
                join r in _directoryContext.Roles on a.RoleId equals r.Id
                where a.ResourceType == RoleResourceType.Portfolio && a.ResourceId == portfolioId
                select new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    Company = u.Company,
                    CreatedDate = u.CreatedDate,
                    Role = new Role { Id = r.Id, Name = r.Name }
                };

            return await query.ToListAsync();
        }

        public async Task<CustomerUserPreferences> GetCustomerUserPreferences(Guid customerUserId)
        {
            var customerUserPreferences = await _directoryContext.CustomerUserPreferences.FindAsync(
                customerUserId
            );

            if (customerUserPreferences == null)
            {
                var defaultPreferences = new CustomerUserPreferencesEntity
                {
                    MobileNotificationEnabled = true,
                    Language = DefaultLanguage
                };

                return CustomerUserPreferencesEntity.MapTo(defaultPreferences);
            }

            return CustomerUserPreferencesEntity.MapTo(customerUserPreferences);
        }

        public async Task CreateOrUpdateCustomerUserPreference(
            Guid customerUserId,
            CustomerUserPreferencesRequest customerUserPreferencesRequest
        )
        {
            var customerUserPreference = await _directoryContext
                .CustomerUserPreferences.AsTracking()
                .Where(u => u.CustomerUserId == customerUserId)
                .FirstOrDefaultAsync();

            if (customerUserPreference != null)
            {
                customerUserPreference.MobileNotificationEnabled =
                    customerUserPreferencesRequest.MobileNotificationEnabled
                    ?? customerUserPreference.MobileNotificationEnabled;
                customerUserPreference.Language =
                    customerUserPreferencesRequest.Language
                    ?? customerUserPreference.Language
                    ?? DefaultLanguage;
                UpdateUserProfile(customerUserPreferencesRequest, customerUserPreference);
            }
            else
            {
                customerUserPreference = new CustomerUserPreferencesEntity();
                customerUserPreference.CustomerUserId = customerUserId;
                customerUserPreference.MobileNotificationEnabled =
                    customerUserPreferencesRequest.MobileNotificationEnabled ?? true;
                customerUserPreference.Language =
                    customerUserPreferencesRequest.Language ?? DefaultLanguage;
                UpdateUserProfile(customerUserPreferencesRequest, customerUserPreference);

                _directoryContext.CustomerUserPreferences.Add(customerUserPreference);
            }

            await _directoryContext.SaveChangesAsync();
        }

        private static void UpdateUserProfile(
            CustomerUserPreferencesRequest customerUserPreferencesRequest,
            CustomerUserPreferencesEntity customerUserPreferencesEntity
        )
        {
            // this check is needed to ensure that mobile/language requests won't clear the user profile settings
            if (customerUserPreferencesRequest.Profile.ValueKind == JsonValueKind.Undefined)
            {
                customerUserPreferencesEntity.Profile =
                    customerUserPreferencesEntity.Profile
                    ?? throw new BadRequestException("Missing Profile");
            }
            else
            {
                try
                {
                    customerUserPreferencesEntity.Profile = JsonSerializerExtensions.Serialize(
                        customerUserPreferencesRequest.Profile
                    );
                }
                catch (JsonException)
                {
                    throw new BadRequestException("Invalid JSON");
                }
            }
        }

        public async Task<CustomerUserTimeSeriesDto> GetCustomerUserTimeSeries(Guid customerUserId)
        {
            var customerUserTimeSeries = await _directoryContext.CustomerUserTimeSeries.FindAsync(
                customerUserId
            );

            return CustomerUserTimeSeriesEntity.MapTo(customerUserTimeSeries);
        }

        public async Task CreateOrUpdateCustomerUserTimeSeries(
            Guid customerUserId,
            CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest
        )
        {
            var customerUserTimeSeries = await _directoryContext
                .CustomerUserTimeSeries.AsTracking()
                .Where(u => u.CustomerUserId == customerUserId)
                .FirstOrDefaultAsync();

            if (customerUserTimeSeries != null)
            {
                UpdateCustomerUserTimeSeries(customerUserTimeSeriesRequest, customerUserTimeSeries);
            }
            else
            {
                customerUserTimeSeries = new CustomerUserTimeSeriesEntity();
                customerUserTimeSeries.CustomerUserId = customerUserId;
                UpdateCustomerUserTimeSeries(customerUserTimeSeriesRequest, customerUserTimeSeries);

                _directoryContext.CustomerUserTimeSeries.Add(customerUserTimeSeries);
            }

            await _directoryContext.SaveChangesAsync();
        }

        private static void UpdateCustomerUserTimeSeries(
            CustomerUserTimeSeriesRequest customerUserTimeSeriesRequest,
            CustomerUserTimeSeriesEntity customerUserTimeSeries
        )
        {
            foreach (
                PropertyInfo property in customerUserTimeSeriesRequest.GetType().GetProperties()
            )
            {
                JsonElement prop = (JsonElement)
                    property.GetValue(customerUserTimeSeriesRequest, null);
                string value = null;

                if (prop.ValueKind != JsonValueKind.Undefined)
                {
                    try
                    {
                        value = JsonSerializerExtensions.Serialize(prop);
                    }
                    catch (JsonException)
                    {
                        throw new BadRequestException("Invalid JSON");
                    }
                }

                customerUserTimeSeries
                    .GetType()
                    .GetProperty(property.Name)
                    .SetValue(customerUserTimeSeries, value);
            }
        }

        public async Task<List<User>> GetFullNamesByUserIdsAync(List<Guid> userIds)
        {
            var users = await _directoryContext
                .Users.Where(u => userIds.Contains(u.Id))
                .Select(
                    x =>
                        new User
                        {
                            Id = x.Id,
                            FirstName = x.FirstName,
                            LastName = x.LastName
                        }
                )
                .ToListAsync();

            return users;
        }

        public async Task<List<User>> GetUsersProfilesAsync(
            GetUsersProfilesRequest getUserProfileRequest
        )
        {
            if (!getUserProfileRequest.Ids.Any() && !getUserProfileRequest.Emails.Any())
            {
                return new List<User>();
            }
            getUserProfileRequest.Ids = getUserProfileRequest.Ids.Distinct().ToList();
            getUserProfileRequest.Emails = getUserProfileRequest.Emails.Distinct().ToList();
            var users = await _directoryContext
                .Users.Where(
                    u =>
                        getUserProfileRequest.Ids.Contains(u.Id)
                        || getUserProfileRequest.Emails.Contains(u.Email)
                )
                .Select(
                    x =>
                        new User
                        {
                            Id = x.Id,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            Mobile = x.Mobile,
                            Company = x.Company,
                        }
                )
                .ToListAsync();

            return users;
        }

        public async Task<UserDetailsDto> GetUserDetailsAsync(Guid userId)
        {
            var userDetails = await _directoryContext
                .Users.Where(x => x.Id == userId && x.Status == UserStatus.Active)
                .Join(
                    _directoryContext.Customers,
                    user => user.CustomerId,
                    customer => customer.Id,
                    (user, customer) => UserDetailsDto.MapFrom(user, customer, _imagePathHelper)
                )
                .FirstOrDefaultAsync();

            if (userDetails is null)
            {
                throw new ResourceNotFoundException("user", userId);
            }
            var permissionIds = new List<string>
            {
                Permissions.ViewSites,
                Permissions.ManageFloors
            };
            var userAssignments = await _directoryContext
                .Assignments.Where(a => a.PrincipalId == userId)
                .Join(
                    _directoryContext.RolePermission.Where(
                        r => permissionIds.Contains(r.PermissionId)
                    ),
                    assignment => assignment.RoleId,
                    rolePermission => rolePermission.RoleId,
                    (assignment, rolePermission) =>
                        new UserAssignment(rolePermission.PermissionId, assignment.ResourceId)
                )
                .ToListAsync();

            userDetails.UserAssignments.AddRange(userAssignments);
            return userDetails;
        }
    }
}
