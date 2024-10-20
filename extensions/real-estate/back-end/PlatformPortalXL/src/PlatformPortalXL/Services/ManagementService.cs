using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using PlatformPortalXL.Auth.Services;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;

using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using PlatformPortalXL.Models;

namespace Willow.Management
{
    public class ManagementService : IManagementService
    {
        private readonly IManagementAccessService _accessService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;
        private readonly IManagedUserRequestValidator _requestValidator;
        private readonly IAuthFeatureFlagService _featureFlagService;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly INotificationService _notificationService;
        private readonly string _commandPortalBaseUrl;
        private readonly IAccessControlService _accessControlService;

        public ManagementService(IManagementAccessService accessService,
                                 IDirectoryApiService directoryApi,
                                 ISiteApiService siteApi,
                                 IManagedUserRequestValidator requestValidator,
                                 IAuthFeatureFlagService featureFlagService,
                                 INotificationService notificationService,
                                 string commandPortalBaseUrl,
                                 IImageUrlHelper imageUrlHelper,
                                 IAccessControlService accessControlService)
        {
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));
            _directoryApi = directoryApi ?? throw new ArgumentNullException(nameof(directoryApi));
            _siteApi = siteApi ?? throw new ArgumentNullException(nameof(siteApi));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _featureFlagService = featureFlagService;
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _commandPortalBaseUrl = commandPortalBaseUrl;
            _imageUrlHelper = imageUrlHelper ?? throw new ArgumentNullException(nameof(imageUrlHelper));
            _accessControlService = accessControlService;
        }

        #region IManagementService

        public async Task<ManagedUserDto> GetManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId)
        {
            // TODO FGA: Users should be managed via UM, this will be retired when UM is in place

            var user = await _accessService.EnsureAccessUser(customerId, currentUserId, managedUserId);
            var managedUser = ManagedUserDto.Map(user.User);

            managedUser.IsCustomerAdmin = user.RoleAssignments?.IsCustomerAdmin(customerId) ?? false;

            if (!managedUser.IsCustomerAdmin)
            {
                managedUser.Portfolios = await GetManagedPortfolios(customerId, user.RoleAssignments);
            }

            return managedUser;
        }

        public async Task<ManagedUserDto> CreateManagedUser(Guid customerId, Guid currentUserId, CreateManagedUserRequest request, string language)
        {
            // TODO FGA: Users should be managed via UM, this will be retired when UM is in place

            await _accessService.EnsureCanCreateUser(customerId, currentUserId);

            await ValidateEmail(request.Email);

            var newUserAssignments = await ValidateUserRequest(request, currentUserId, customerId, null, false);

            var user = await _directoryApi.CreateCustomerUser(customerId,
                new DirectoryCreateCustomerUserRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Mobile = request.ContactNumber,
                    Company = request.Company,
                    UseB2C = true
                });

            return await PostCreateUpdate(customerId, user, newUserAssignments, request, language);
        }

        public async Task UpdateManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId, UpdateManagedUserRequest request, string language)
        {
            // TODO FGA: Users should be managed via UM, this will be retired when UM is in place

            var user = await _accessService.EnsureAccessUser(customerId, currentUserId, managedUserId);
            var newUserAssignments = await ValidateUserRequest(request, currentUserId, customerId, user.RoleAssignments, true);

            await _directoryApi.UpdateCustomerUser(customerId, managedUserId,
                new DirectoryUpdateCustomerUserRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Mobile = request.ContactNumber,
                    Company = request.Company,
                });

            await PostCreateUpdate(customerId, user.User, newUserAssignments, request, language);
        }

        public async Task DeleteManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId)
        {
            // TODO FGA: Users should be managed via UM, this will be retired when UM is in place

            await _accessService.EnsureAccessUser(customerId, currentUserId, managedUserId, true);

            await _directoryApi.DeleteCustomerUser(customerId, managedUserId);
        }

        public async Task<List<ManagedPortfolioDto>> GetManagedPortfolios(Guid customerId, Guid userId)
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                return await GetManagedPortfoliosViaFGA(customerId, userId);
            }

            var assignments = await _directoryApi.GetRoleAssignments(userId);

            return await GetManagedPortfolios(customerId, assignments);
        }

        public async Task<List<ManagedPortfolioDto>> GetManagedPortfolios(Guid customerId, List<RoleAssignmentDto> assignments)
        {
            var portfolios = await GetPortfolios(customerId);

            return portfolios.MapToManagedPortfolioDto(assignments, _imageUrlHelper);
        }

        #endregion

        #region Private

        private async Task<ManagedUserDto> PostCreateUpdate(Guid customerId, User user, IList<RoleAssignment> newUserAssignments, ManagedUserRequest request, string language)
        {
            var managedUser = ManagedUserDto.Map(user);

            // Update all the user assignments
            await _directoryApi.CreateUserAssignments(user.Id, newUserAssignments);

            managedUser.IsCustomerAdmin = newUserAssignments.IsCustomerAdmin(customerId);

            var assignedSiteNames = managedUser.IsCustomerAdmin ? new List<string>() : GetAssignedSiteNames(managedUser, request);
            if (managedUser.IsCustomerAdmin || assignedSiteNames.Any())
            {
                await SendEmail(customerId, assignedSiteNames, user.Id, managedUser.IsCustomerAdmin, language,user.Type.ToString());
            }

            return managedUser;
        }

        private const string AdminRoleName = "Admin";
        private const string ViewerRoleName = "Viewer";

        private async Task<List<ManagedPortfolioDto>> GetManagedPortfoliosViaFGA(Guid customerId, Guid userId)
        {
            var portfolios = await GetPortfolios(customerId);

            var result = new List<ManagedPortfolioDto>();
            foreach (var portfolio in portfolios)
            {
                var managedSites = await GetManagedSitesViaFGA(portfolio.Sites, userId);
                if (managedSites.Count == 0)
                {
                    continue;
                }

                var canManagePortfolios = await _accessControlService.CanAccessPortfolio(userId, Permissions.ManagePortfolios, portfolio.Id);
                result.Add(new ManagedPortfolioDto
                {
                    PortfolioId = portfolio.Id,
                    PortfolioName = portfolio.Name,
                    Role = canManagePortfolios ? AdminRoleName : ViewerRoleName,
                    Features = PortfolioFeaturesDto.MapFrom(portfolio.Features),
                    Sites = managedSites
                });
            }

            return result;
        }

        private async Task<List<ManagedSiteDto>> GetManagedSitesViaFGA(List<Site> sites, Guid userId)
        {
            var result = new List<ManagedSiteDto>();
            foreach (var site in sites)
            {
                if (await _accessControlService.CanAccessSite(userId, Permissions.ViewSites, site.Id))
                {
                    result.Add(new ManagedSiteDto
                    {
                        SiteId = site.Id,
                        SiteName = site.Name,
                        Role = await _accessControlService.CanAccessSite(userId, Permissions.ManageSites, site.Id) ? AdminRoleName : ViewerRoleName,
                        LogoUrl = site.LogoId.HasValue ? _imageUrlHelper.GetSiteLogoUrl(site.LogoPath, site.LogoId.Value) : null,
                        LogoOriginalSizeUrl = site.LogoId.HasValue
                        ? _imageUrlHelper.GetSiteLogoOriginalSizeUrl(site.LogoPath, site.LogoId.Value)
                        : null
                    });
                }
            }

            return result;
        }

        private async Task<List<Portfolio>> GetPortfolios(Guid customerId)
        {
            var portfolios = await _directoryApi.GetCustomerPortfolios(customerId, includeSites: true) ?? [];

            if (portfolios.Count == 0)
            {
                return portfolios;
            }

            var sites = await _siteApi.GetSites(portfolios.SelectMany(x => x.Sites ?? []).Select(x => x.Id));
            var groupedSites = sites.GroupBy(x => x.PortfolioId).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var portfolio in portfolios.Where(x => groupedSites.ContainsKey(x.Id)))
            {
                portfolio.Sites = groupedSites[portfolio.Id];
            }

            return portfolios;
        }

        private async Task ValidateEmail(string email)
        {
            var account = await _directoryApi.GetAccount(email);

            if (account != null)
                throw new ArgumentException("User already exists", nameof(email)).WithData(new { email });
        }

        private async Task SendEmail(Guid customerId, List<string> siteNames, Guid userId, bool isSuperAdmin, string language,string userType)
        {
            if (isSuperAdmin)
            {
                var parameters = new
                {
                    LoginUrl = _commandPortalBaseUrl
                };
                await _notificationService.SendNotificationAsync(new Willow.Notifications.Models.Notification
                {
                    CorrelationId = Guid.NewGuid(),
                    CommunicationType = CommunicationType.Email,
                    CustomerId = customerId,
                    Data = parameters.ToDictionary(),
                    Tags = null,
                    TemplateName = "AssignSuperUserRole",
                    UserId = userId,
                    UserType = userType,
                    Locale = language

                });
            }
            else
            {
                var parameters = new
                {
                    LoginUrl = _commandPortalBaseUrl,
                    SitesInTitle = GetEmailSitesInTitle(siteNames, 3),
                    SitesInBody = GetEmailSitesInBody(siteNames),
                    SitesLabel = siteNames.Count > 1 ? "sites" : "site"
                };
                await _notificationService.SendNotificationAsync(new Willow.Notifications.Models.Notification
                {
                    CorrelationId = Guid.NewGuid(),
                    CommunicationType = CommunicationType.Email,
                    CustomerId = customerId,
                    Data = parameters.ToDictionary(),
                    Tags = null,
                    TemplateName = "SiteAssigned",
                    UserId = userId,
                    UserType = userType,
                    Locale = language

                });
            }
        }

        private static string GetEmailSitesInTitle(List<string> siteNames, int maxSitesInTitle)
        {
            var count = siteNames.Count;
            var siteListInTitle = HttpUtility.HtmlEncode(string.Join(',', siteNames.ToArray(), 0, Math.Min(count, maxSitesInTitle)));

            return count > maxSitesInTitle ? $"{siteListInTitle}..." : siteListInTitle;
        }

        private static string GetEmailSitesInBody(List<string> siteNames)
        {
            var siteListInBody = new StringBuilder();

            foreach (var site in siteNames)
            {
                siteListInBody.Append($"<li>{HttpUtility.HtmlEncode(site)}</li>");
            }

            return siteListInBody.ToString();
        }

        private async Task<IList<RoleAssignment>> ValidateUserRequest(ManagedUserRequest request, Guid currentUserId, Guid customerId, IList<RoleAssignmentDto> managedUserAssignments, bool allowNone)
        {
            var currentUserAssignments = await _directoryApi.GetRoleAssignments(currentUserId);

            return await _requestValidator.Validate(request, customerId, currentUserAssignments, managedUserAssignments, allowNone);
        }

        private static List<string> GetAssignedSiteNames(ManagedUserDto user, ManagedUserRequest request)
        {
            var assignedSiteNames = new List<string>();

            if (!user.IsCustomerAdmin)
            {
                foreach (var portfolio in request.Portfolios)
                {
                    var managedSites = new List<ManagedSiteDto>();

                    foreach (var site in portfolio.Sites)
                    {
                        managedSites.Add(site);
                        if (!string.IsNullOrEmpty(site.Role))
                        {
                            assignedSiteNames.Add(site.SiteName);
                        }
                    }

                    user.Portfolios.Add(new ManagedPortfolioDto
                    {
                        PortfolioId = portfolio.PortfolioId,
                        PortfolioName = portfolio.PortfolioName,
                        Role = portfolio.Role,
                        Sites = managedSites
                    });
                }
            }

            return assignedSiteNames;
        }

        #endregion
    }
}
