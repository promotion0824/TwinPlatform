using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Willow.Data;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Management.UnitTests
{
    public class ManagementAccessServiceTests
    {
        private readonly IManagementAccessService _accessService;
        private readonly Mock<IDirectoryApiService> _directoryApi = new Mock<IDirectoryApiService>();
        private readonly Mock<IReadRepository<Guid, User>> _userService = new Mock<IReadRepository<Guid, User>>();
        private readonly Mock<IReadRepository<Guid, Site>> _siteRepo = new Mock<IReadRepository<Guid, Site>>();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _managedUserId = Guid.NewGuid();
        private readonly Guid _currentUserId = Guid.NewGuid();
        private readonly Guid _inactiveUserId = Guid.NewGuid();
        private readonly Guid _portfolioId = Guid.NewGuid();
        private readonly Guid _siteId = Guid.NewGuid();

        public ManagementAccessServiceTests()
        {
            _siteRepo.Setup( r=> r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, Name = "Bedrock", PortfolioId = _portfolioId});

            _userService.Setup( u=> u.Get(_currentUserId)).ThrowsAsync(new NotFoundException());
            _userService.Setup( u=> u.Get(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());
            
            _userService.Setup( u=> u.Get(_managedUserId)).ReturnsAsync(new User
            {
                Id         = _managedUserId,
                FirstName  = "Fred",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

           _userService.Setup( u=> u.Get(_inactiveUserId)).ReturnsAsync(new User
            {
                Id         = _inactiveUserId,
                FirstName  = "Barney",
                LastName   = "Rubble",
                CustomerId = _customerId,
                Status     = UserStatus.Pending
            });

           _userService.Setup( u=> u.Get(_currentUserId)).ReturnsAsync(new User
            {
                Id         = _currentUserId,
                FirstName  = "Wilma",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

            _directoryApi.Setup( api=> api.GetCustomer(_customerId)).ReturnsAsync( new Customer { Id = _customerId, Name = "Granite City"});
            _accessService = new ManagementAccessService(_directoryApi.Object, _userService.Object);
        }

        #region EnsureAccessUser

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_accessdenied_wrong_customer()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            var differentCustomerId = Guid.NewGuid();

            _userService.Setup( u=> u.Get(_currentUserId)).ThrowsAsync(new NotFoundException());

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = differentCustomerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });
            
            try
            {
                await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId);
                Assert.True(false, "Expected UnauthorizedAccessException");
            } 
            catch(UnauthorizedAccessException ex)
            {
                Assert.Equal(_customerId.ToString(), ex.Data["CustomerId"].ToString());
                Assert.Equal(_currentUserId.ToString(), ex.Data["CurrentUserId"].ToString());
                Assert.Equal(_managedUserId.ToString(), ex.Data["ManagedUserId"].ToString());
            }
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_customeradmin_success()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = _customerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });

            var result = await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(result.RoleAssignments); // ???
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_portfolioadmin_success()
        {            
            var portfolioId = Guid.NewGuid();

            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioViewer,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = portfolioId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = portfolioId
                }
            });

            var result = await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(result.RoleAssignments); // ???
        }                      
        
        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_portfolioadmin_site_success()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = Guid.NewGuid()
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            var result = await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(result.RoleAssignments);
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_notportfolioadmin_site_fail()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = Guid.NewGuid()
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioViewer,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId));
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_siteadmin_site_success()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = Guid.NewGuid()
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.SiteAdmin,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            var result = await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(result.RoleAssignments);
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_currentuser_notsiteadmin()
        {            
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = Guid.NewGuid()
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId));
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_accessdenied_customer_admin_mismatch()
        {            
            // Managed user is customer admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = _customerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });

            // Current user is not
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = Guid.NewGuid(),
                    PortfolioId  = _portfolioId
                }
            });
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureAccessUser(_customerId, _currentUserId, _managedUserId));
        }

        [Fact]
        public async Task ManagementAccessService_EnsureAccessUser_accessdenied_inactiveuser()
        {            
            // Managed user is customer admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = _customerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });

            // Current user is not
            _directoryApi.Setup( d=> d.GetRoleAssignments(_inactiveUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _inactiveUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureAccessUser(_customerId, _inactiveUserId, _managedUserId));
        }

        #endregion

        #region EnsureCanCreateUser

        [Fact]
        public async Task ManagementAccessService_EnsureCanCreateUser_accessdenied_not_admin()
        {            
            // Current user is not admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioViewer,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureCanCreateUser(_customerId, _currentUserId));
        }

        [Fact]
        public async Task ManagementAccessService_EnsureCanCreateUser_accessdenied_customer_mismatch()
        {            
            var wrongCustomerId = Guid.NewGuid();

            // Current user is not admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = wrongCustomerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = wrongCustomerId
                }
            });
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _accessService.EnsureCanCreateUser(_customerId, _currentUserId));
        }

        [Fact]
        public async Task ManagementAccessService_EnsureCanCreateUser_customeradmin_success()
        {            
            // Current user is not admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = _customerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });
            
            await _accessService.EnsureCanCreateUser(_customerId, _currentUserId);
        }

        [Fact]
        public async Task ManagementAccessService_EnsureCanCreateUser_portfolioadmin_success()
        {            
            // Current user is not admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });
            
            await _accessService.EnsureCanCreateUser(_customerId, _currentUserId);
        }

        [Fact]
        public async Task ManagementAccessService_EnsureCanCreateUser_siteadmin_success()
        {            
            // Current user is not admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.SiteAdmin,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });
            
            await _accessService.EnsureCanCreateUser(_customerId, _currentUserId);
        }

        #endregion
    }
}
