using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Services
{
    public interface IPersonManagementService
    {
        Task<List<PersonDetailDto>> GetUsersBasedOnUserPermission(Guid userId);
    }

    public class PersonManagementService : IPersonManagementService
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ILogger<PersonManagementService> _logger;

        public PersonManagementService(
            IAccessControlService accessControl,
            IDirectoryApiService directoryApi,
            IWorkflowApiService workflowApi,
            ILogger<PersonManagementService> logger)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _workflowApi = workflowApi;
            _logger = logger;
        }

        public async Task<List<PersonDetailDto>> GetUsersBasedOnUserPermission(Guid userId)
        {
            // TODO: Users should be managed via UM, this will be retired when UM is in place
            var userDict = new Dictionary<Guid, PersonDetailDto>();
            var user = await _directoryApi.GetUser(userId);

            var canAccessCustomer = await _accessControl.CanAccessCustomer(userId, Permissions.ViewUsers, user.CustomerId);
            if (canAccessCustomer)
            {
                var allCustomerUsers = await _directoryApi.GetCustomerUsers(user.CustomerId);
                userDict = allCustomerUsers
                    .Where(x => x.Status != UserStatus.Deleted && x.Status != UserStatus.Inactive)
                    .ToDictionary(k => k.Id, k => PersonDetailDto.Map(k));
            }

            var allPortfolios = await _directoryApi.GetCustomerPortfolios(user.CustomerId, false);
            foreach (var portfolio in allPortfolios)
            {
                var portfolioSimpleDto = new PortfolioSimpleDto { Id = portfolio.Id, Name = portfolio.Name };
                var canAccessPortfolio = await _accessControl.CanAccessPortfolio(userId, Permissions.ViewUsers, portfolio.Id);
                if (canAccessPortfolio)
                {
                    var portfolioUsers = (await _directoryApi.GetPortfolioUsers(portfolio.Id))
                        .Where(x => x.Status != UserStatus.Deleted && x.Status != UserStatus.Inactive ).ToList();
                    portfolioUsers.ForEach(u => AddPersonIfNotExists(userDict, u.Id, PersonDetailDto.Map(u), portfolio : portfolioSimpleDto));
                }
                var allPortfolioSites = await _directoryApi.GetPortfolioSites(user.CustomerId, portfolio.Id);
                foreach (var pSite in allPortfolioSites)
                {
                    try
                    {
                        var canAccessSite = await _accessControl.CanAccessSite(userId, Permissions.ViewUsers, pSite.Id);
                        if (canAccessSite)
                        {
                            var siteUsers = (await _directoryApi.GetSiteUsers(pSite.Id))
                                .Where(x => x.Status != UserStatus.Deleted && x.Status != UserStatus.Inactive ).ToList();

                            var siteReporters = await _workflowApi.GetReporters(pSite.Id);
                            var siteSimpleDto = new SiteSimpleDto { Id = pSite.Id, Name = pSite.Name, Portfolio = portfolioSimpleDto };
                            siteUsers.ForEach(u => AddPersonIfNotExists(userDict, u.Id, PersonDetailDto.Map(u), site : siteSimpleDto));
                            siteReporters.ForEach(r => AddPersonIfNotExists(userDict, r.Id, PersonDetailDto.Map(r), site: siteSimpleDto));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error getting users for site {SiteId}", pSite.Id);
                    }
                }
            }
            return userDict.Values.ToList();
        }

        private static void AddPersonIfNotExists(Dictionary<Guid, PersonDetailDto> users,
                                          Guid personId,
                                          PersonDetailDto person,
                                          PortfolioSimpleDto portfolio = null,
                                          SiteSimpleDto site = null)
        {
            if (!users.ContainsKey(personId))
            {
                users.Add(personId, person);
            }
            if (portfolio != null)
            {
                users[personId].Portfolios.Add(portfolio);
            }
            if (site != null)
            {
                users[personId].Sites.Add(site);
            }
        }
    }
}
