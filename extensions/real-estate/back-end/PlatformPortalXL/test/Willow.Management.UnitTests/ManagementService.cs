using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using PlatformPortalXL.Auth.Services;
using Willow.Data;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.ExceptionHandling.Exceptions;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace Willow.Management.UnitTests
{
    public class ManagementServiceTests
    {
        private readonly IManagementService _managementService;
        private readonly IManagedUserRequestValidator _requestValidator;
        private readonly Mock<IDirectoryApiService> _directoryApi = new Mock<IDirectoryApiService>();
        private readonly Mock<ISiteApiService> _siteApi = new Mock<ISiteApiService>();
        private readonly Mock<IReadRepository<Guid, User>> _userRepo = new Mock<IReadRepository<Guid, User>>();
        private readonly Mock<IReadRepository<Guid, Site>> _siteRepo = new Mock<IReadRepository<Guid, Site>>();
        private readonly Mock<INotificationService> _emailServer = new Mock<INotificationService>();
        private readonly Mock<IImageUrlHelper> _imageUrlHelper = new Mock<IImageUrlHelper>();
        private readonly Mock<IAccessControlService> _accessControlService = new Mock<IAccessControlService>();
        private readonly Mock<IAuthFeatureFlagService> _featureFlagService = new Mock<IAuthFeatureFlagService>();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _managedUserId = Guid.NewGuid();
        private readonly Guid _currentUserId = Guid.NewGuid();
        private readonly Guid _portfolioId = Guid.NewGuid();
        private readonly Guid _siteId = Guid.NewGuid();
        private readonly UpdateManagedUserRequest _updateRequest;

        public ManagementServiceTests()
        {
            _requestValidator = new ManagedUserRequestValidator(_siteRepo.Object);

            _managementService = new ManagementService(new ManagementAccessService(_directoryApi.Object, _userRepo.Object),
                                                       _directoryApi.Object,
                                                       _siteApi.Object,
                                                       _requestValidator,
                                                       _featureFlagService.Object,
                                                       _emailServer.Object,
                                                       "blah",
                                                       _imageUrlHelper.Object,
                                                       _accessControlService.Object);

            _siteRepo.Setup( r=> r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, Name = "Bedrock", PortfolioId = _portfolioId, CustomerId = _customerId});

            _userRepo.Setup( u=> u.Get(_currentUserId)).ThrowsAsync(new NotFoundException());
            _userRepo.Setup( u=> u.Get(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException());

            _userRepo.Setup( u=> u.Get(_managedUserId)).ReturnsAsync(new User
            {
                Id         = _managedUserId,
                FirstName  = "Fred",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

           _userRepo.Setup( u=> u.Get(_currentUserId)).ReturnsAsync(new User
            {
                Id         = _currentUserId,
                FirstName  = "Wilma",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

            _directoryApi.Setup( api=> api.GetCustomer(_customerId)).ReturnsAsync( new Customer { Id = _customerId, Name = "Granite City"});

            _updateRequest = new UpdateManagedUserRequest
            {
                FirstName       = "Fred",
                LastName        = "Flintstone",
                Company         = "Slate Rock and Gravel Company",
                ContactNumber   = "555.555.5555",
                IsCustomerAdmin = false
            };
        }

        #region Get

        [Fact]
        public async Task ManagementService_Get_notfound()
        {
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

            await Assert.ThrowsAsync<NotFoundException>( async ()=> await _managementService.GetManagedUser(_customerId, _currentUserId, Guid.NewGuid()));
        }

        [Fact]
        public async Task ManagementService_Get_accessdenied_wrong_customer()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            var differentCustomerId = Guid.NewGuid();

            _userRepo.Setup( u=> u.Get(_currentUserId)).ThrowsAsync(new NotFoundException());

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = differentCustomerId,
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });

            try
            {
                await _managementService.GetManagedUser(_customerId, _currentUserId, _managedUserId);
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
        public async Task ManagementService_Get_currentuser_customeradmin_success()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
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

            var managedUser = await _managementService.GetManagedUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(managedUser);
        }

        [Fact]
        public async Task ManagementService_Get_currentuser_portfolioadmin_success()
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
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            var managedUser = await _managementService.GetManagedUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(managedUser);
        }

        [Fact]
        public async Task ManagementService_Get_currentuser_portfolioadmin_site_success()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            var managedUser = await _managementService.GetManagedUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(managedUser);
        }

        [Fact]
        public async Task ManagementService_Get_accessdenied_customer_admin_mismatch()
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
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            var user = await _managementService.GetManagedUser(_customerId, _currentUserId, _managedUserId);

            Assert.NotNull(user);
        }

        #endregion

        #region CreateManagedUser

        [Fact]
        public async Task ManagementService_CreateManagedUser_success()
        {
           _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

           _directoryApi.Setup( d=> d.CreateCustomerUser(_customerId, It.IsAny<DirectoryCreateCustomerUserRequest>())).ReturnsAsync(new User
            {
                Id         = _managedUserId,
                FirstName  = "Fred",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

            _userRepo.Setup( u=> u.Get(It.IsAny<Guid>())).ReturnsAsync(new User
            {
                Id         = _managedUserId,
                FirstName  = "Fred",
                LastName   = "Flintstone",
                CustomerId = _customerId,
                Status     = UserStatus.Active
            });

            await _managementService.CreateManagedUser(_customerId, _currentUserId, new CreateManagedUserRequest
            {
                FirstName       = "Fred",
                LastName        = "Flintstone",
                Company         = "Slate Rock and Gravel Company",
                ContactNumber   = "555.555.5555",
                Email           = "fred.flintstone@bedrock.com",
                IsCustomerAdmin = false,
                Portfolios      = new List<ManagedPortfolioDto>
                {
                    new ManagedPortfolioDto
                    {
                        PortfolioId    = _portfolioId,
                        PortfolioName  = "Granite Co",
                        Role           = "Viewer",
                        Sites          = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId   = _siteId,
                                SiteName = "North Quarry",
                                Role     = "Viewer"
                            }
                        }
                    }
                }
            },
            "en");

            _emailServer.Verify( e=> e.SendNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        #endregion

        #region UpdateManagedUser

        [Fact]
        public async Task ManagementService_UpdateManagedUser_notfound()
        {
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

            await Assert.ThrowsAsync<NotFoundException>( async ()=> await _managementService.UpdateManagedUser(_customerId, _currentUserId, Guid.NewGuid(), _updateRequest, "en" ));
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_accessdenied_no_user_assignments()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, null, null, null)).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en" ));
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_accessdenied_wrong_customer()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.CustomerAdmin,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Customer,
                    CustomerId   = _customerId
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en" ));
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentuser_customeradmin_success()
        {
            _siteRepo.Setup( r=> r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
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

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = "Viewer"
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify( api=> api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
            _emailServer.Verify( s=> s.SendNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentuser_portfolioadmin_success()
        {
            var portfolioId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioViewer,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = portfolioId,
                    PortfolioName = "Stone Co",
                    Role = "Viewer",
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = "Viewer"
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify( api=> api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_accessdenied_wrong_portfolio()
        {
            var wrongPortfolioId = Guid.NewGuid();

            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId,
                    PortfolioId  = _portfolioId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = wrongPortfolioId,
                    Role = "Viewer"
                }
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en"));
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentuser_portfolioadmin_site_success()
        {
            _siteRepo.Setup( r=> r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = _portfolioId,
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = "Viewer",
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify( api=> api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_accessdenied_customer_admin_mismatch()
        {
            // Managed user is customer admin
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
            });

            // Current user is not
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = _portfolioId,
                    Role = "Viewer"
                }
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en"));
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_changeportfolios_succeeds()
        {
            var oldPortfolioId = Guid.NewGuid();

            // Current managed user assignments
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = oldPortfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            // Current user's assignments
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = oldPortfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = _portfolioId,
                    Role = "Viewer",
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = "Viewer"
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify( api=> api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_changeportfolios()
        {
            var oldPortfolioId = Guid.NewGuid();

            // Current managed user assignments
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = oldPortfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            // Current user's assignments
            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = Guid.NewGuid(),
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                },
                new RoleAssignmentDto
                {
                    PrincipalId  = _currentUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = _portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    CustomerId   = _customerId
                }
            });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    PortfolioId = _portfolioId,
                    Role = "Viewer",
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = "Viewer"
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentUser_customerAdmin_withNoSiteAssign_DontSendEmail_Success()
        {
            _siteRepo.Setup(r => r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup(d => d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup(d => d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
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

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    Sites = new List<ManagedSiteDto>
                    {
                        new ManagedSiteDto
                        {
                            SiteId = _siteId,
                            SiteName = "Bedrock",
                            Role = ""
                        }
                    }
                }
            };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify(api => api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);

            _emailServer.Verify(s => s.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
        }

        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentUser_WithNoPortfolioAndSites_DontSendEmail_success()
        {
            var portfolioId = Guid.NewGuid();

            _siteRepo.Setup(r => r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup(d => d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
                {
                    new RoleAssignmentDto
                    {
                        PrincipalId  = _managedUserId,
                        RoleId       = WellKnownRoleIds.PortfolioViewer,
                        ResourceId   = portfolioId,
                        ResourceType = RoleResourceType.Portfolio,
                        CustomerId   = _customerId
                    }
                });

            _directoryApi.Setup(d => d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
                {
                    new RoleAssignmentDto
                    {
                        PrincipalId  = _managedUserId,
                        RoleId       = WellKnownRoleIds.PortfolioAdmin,
                        ResourceId   = portfolioId,
                        ResourceType = RoleResourceType.Portfolio,
                        CustomerId   = _customerId
                    }
                });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
                {
                    new ManagedPortfolioDto
                    {
                        PortfolioId = portfolioId,
                        PortfolioName = "Stone Co",
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = _siteId,
                                SiteName = "Bedrock",
                                Role = ""
                            }
                        }
                    }
                };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify(api => api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
            _emailServer.Verify(s => s.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
        }


        [Fact]
        public async Task ManagementService_UpdateManagedUser_currentUser_portfolioAdmin_DontSendEmail_NoSite_success()
        {
            _siteRepo.Setup(r => r.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId });

            _directoryApi.Setup(d => d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
                {
                    new RoleAssignmentDto
                    {
                        PrincipalId  = _managedUserId,
                        RoleId       = WellKnownRoleIds.SiteViewer,
                        ResourceId   = _siteId,
                        ResourceType = RoleResourceType.Site,
                        CustomerId   = _customerId
                    }
                });

            _directoryApi.Setup(d => d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
                {
                    new RoleAssignmentDto
                    {
                        PrincipalId  = _managedUserId,
                        RoleId       = WellKnownRoleIds.PortfolioAdmin,
                        ResourceId   = _portfolioId,
                        ResourceType = RoleResourceType.Portfolio,
                        CustomerId   = _customerId
                    }
                });

            _updateRequest.Portfolios = new List<ManagedPortfolioDto>
                {
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = _siteId,
                                SiteName = "Bedrock",
                                Role = null,
                            }
                        }
                    }
                };

            await _managementService.UpdateManagedUser(_customerId, _currentUserId, _managedUserId, _updateRequest, "en");

            _directoryApi.Verify(api => api.UpdateCustomerUser(_customerId, _managedUserId, It.IsAny<DirectoryUpdateCustomerUserRequest>()), Times.Once);
            _emailServer.Verify(s => s.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
        }
    #endregion

    #region DeleteManagedUser

    [Fact]
        public async Task ManagementService_DeleteManagedUser_accessdenied_customer_admin_mismatch()
        {
            var portfolioId = Guid.NewGuid();

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
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.PortfolioAdmin,
                    ResourceId   = portfolioId,
                    ResourceType = RoleResourceType.Portfolio,
                    PortfolioId  = portfolioId,
                    CustomerId   = Guid.NewGuid()
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _managementService.DeleteManagedUser(_customerId, _currentUserId, _managedUserId));
        }

        [Fact]
        public async Task ManagementService_DeleteManagedUser_success()
        {
            _directoryApi.Setup( d=> d.GetRoleAssignments(_managedUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto
                {
                    PrincipalId  = _managedUserId,
                    RoleId       = WellKnownRoleIds.SiteViewer,
                    ResourceId   = _siteId,
                    ResourceType = RoleResourceType.Site,
                    CustomerId   = _customerId
                }
            });

            _directoryApi.Setup( d=> d.GetRoleAssignments(_currentUserId, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>())).ReturnsAsync(new List<RoleAssignmentDto>
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

            await _managementService.DeleteManagedUser(_customerId, _currentUserId, _managedUserId);

            _directoryApi.Verify( api=> api.DeleteCustomerUser(_customerId, _managedUserId), Times.Once);
        }

        #endregion
    }
}
