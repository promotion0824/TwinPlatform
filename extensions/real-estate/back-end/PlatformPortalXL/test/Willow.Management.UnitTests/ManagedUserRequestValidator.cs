using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Willow.Data;
using Willow.Directory.Models;
using Willow.Platform.Models;

namespace Willow.Management.UnitTests
{
    public class ManagedUserRequestValidatorTests
    {
        private Mock<IReadRepository<Guid, Site>> _siteRepo = new Mock<IReadRepository<Guid, Site>>();
        private readonly IManagedUserRequestValidator _validator;
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _portfolioId1 = Guid.NewGuid();
        private readonly Guid _portfolioId2 = Guid.NewGuid();
        private readonly Guid _portfolioId3 = Guid.NewGuid();
        private readonly Guid _portfolioId4 = Guid.NewGuid();
        private readonly Guid _portfolioId5 = Guid.NewGuid();
        private readonly Guid _portfolioId6 = Guid.NewGuid();

        public ManagedUserRequestValidatorTests()
        {
            _validator = new ManagedUserRequestValidator(_siteRepo.Object);
        }

        #region Validate_CreateManagedUserRequest

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_null_roles_fails()
        {
            await Assert.ThrowsAsync<ArgumentNullException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false
            },
            _customerId,
            null,
            null,
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_noroles_fails()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false
            },
            _customerId,
            new List<RoleAssignmentDto> { },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_fails()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = true
            },
            _customerId,
            new List<RoleAssignmentDto> { new RoleAssignmentDto { RoleId = WellKnownRoleIds.PortfolioViewer } },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_fails_wrong_customer()
        {
            var wrongCustomerId = Guid.NewGuid();

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = true
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = wrongCustomerId,
                    CustomerId = wrongCustomerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_fails_no_portfolios()
        {
            await Assert.ThrowsAsync<ArgumentException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_fails_wrong_portfolios()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = true,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = Guid.NewGuid(),
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_fails_invalid_role()
        {
            await Assert.ThrowsAsync<ArgumentException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Role = "bob"
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_succeeds()
        {
            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = true
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.CustomerAdmin, result[0].RoleId);
            Assert.Equal(_customerId, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Customer, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_portfolio_admin_succeeds()
        {
            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Role = "Admin"
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.PortfolioAdmin, result[0].RoleId);
            Assert.Equal(_portfolioId1, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Portfolio, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customer_admin_portfolio_succeeds()
        {
            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Role = "Admin"
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.PortfolioAdmin, result[0].RoleId);
            Assert.Equal(_portfolioId1, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Portfolio, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_portfolioadmin_siteviewer_succeeds()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = siteId,
                                SiteName = "Bob's Emporium",
                                Role = "Viewer"
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.SiteViewer, result[0].RoleId);
            Assert.Equal(siteId, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Site, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_siteviewer_succeeds()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = siteId,
                                SiteName = "Bob's Emporium",
                                Role = "Viewer"
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.SiteViewer, result[0].RoleId);
            Assert.Equal(siteId, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Site, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_siteadmin_succeeds()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = siteId,
                                SiteName = "Bob's Emporium",
                                Role = "Viewer"
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId,
                    PortfolioId = _portfolioId1
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.SiteViewer, result[0].RoleId);
            Assert.Equal(siteId, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Site, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_fails_wrong_siteadmin()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            await Assert.ThrowsAsync<UnauthorizedAccessException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = siteId,
                                SiteName = "Bob's Emporium",
                                Role = "Viewer"
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = Guid.NewGuid(),
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_no_assignments()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            await Assert.ThrowsAsync<ArgumentException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_remove_assignment()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new UpdateManagedUserRequest
            {
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            true);

            Assert.Empty(assignments);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_remove_adminrole()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new UpdateManagedUserRequest
            {
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "Viewer",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            true);

            Assert.Single(assignments);
            Assert.Equal(siteId, assignments[0].ResourceId);
            Assert.Equal(RoleResourceType.Site, assignments[0].ResourceType);
            Assert.Equal(WellKnownRoleIds.SiteViewer, assignments[0].RoleId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_remove_assignment_wExisting()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId2,
                    CustomerId = _customerId
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            false);

            Assert.Single(assignments);
            Assert.Equal(siteId2, assignments[0].ResourceId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_update_assignment_wExisting2()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "Viewer",
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "Admin",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioViewer, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId,
                    PortfolioId = _portfolioId1
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId,
                    PortfolioId = _portfolioId1
                } 
            },
            false);

            Assert.Equal(2, assignments.Count);

            // Make sure it hasn't changed
            Assert.Equal(_portfolioId1, assignments[0].ResourceId);
            Assert.Equal(WellKnownRoleIds.PortfolioViewer, assignments[0].RoleId);

            // Make sure upgraded to site admin
            Assert.Equal(siteId, assignments[1].ResourceId);
            Assert.Equal(WellKnownRoleIds.SiteAdmin, assignments[1].RoleId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_update_assignment_wExisting3()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "Viewer",
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "Viewer",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioViewer, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId,
                    PortfolioId = _portfolioId1
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId,
                    PortfolioId = _portfolioId1
                } 
            },
            false);

            Assert.Equal(2, assignments.Count);

            // Make sure it hasn't changed
            Assert.Equal(_portfolioId1, assignments[0].ResourceId);
            Assert.Equal(WellKnownRoleIds.PortfolioViewer, assignments[0].RoleId);

            // Make sure upgraded to site admin
            Assert.Equal(siteId, assignments[1].ResourceId);
            Assert.Equal(WellKnownRoleIds.SiteViewer, assignments[1].RoleId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_upgrade_assignment_wExisting()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "Admin",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteAdmin, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId2,
                    CustomerId = _customerId
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    CustomerId = _customerId
                } 
            },
            false);

            Assert.Equal(2, assignments.Count);
            Assert.Equal(siteId2, assignments[0].ResourceId);
            Assert.Equal(siteId,  assignments[1].ResourceId);
            Assert.Equal(WellKnownRoleIds.SiteAdmin, assignments[1].RoleId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_upgrade_removesites()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });
            _siteRepo.Setup( r=> r.Get(siteId2)).ReturnsAsync(new Site { Id = siteId2, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "Viewer",
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId
                            },
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId2
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId2,
                    PortfolioId = _portfolioId1,
                    CustomerId = _customerId                    
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    PortfolioId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            false);

            Assert.Single(assignments);
            Assert.Equal(_portfolioId1, assignments[0].ResourceId);
            Assert.Equal(RoleResourceType.Portfolio,  assignments[0].ResourceType);
            Assert.Equal(WellKnownRoleIds.PortfolioViewer, assignments[0].RoleId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_upgrade_removesites_wExtrasites()
        {
            var siteId = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();
            var siteId3 = Guid.NewGuid();
            var siteId4 = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });
            _siteRepo.Setup( r=> r.Get(siteId2)).ReturnsAsync(new Site { Id = siteId2, CustomerId = _customerId });

            var assignments = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "Viewer",
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId
                            },
                            new ManagedSiteDto
                            {
                                Role = "",
                                SiteId = siteId2
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.PortfolioAdmin, 
                    ResourceType = RoleResourceType.Portfolio,
                    ResourceId = _portfolioId1,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId2,
                    PortfolioId = _portfolioId1,
                    CustomerId = _customerId                    
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId,
                    PortfolioId = _portfolioId1,
                    CustomerId = _customerId
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId3,
                    PortfolioId = _portfolioId2,
                    CustomerId = _customerId                    
                } ,
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.SiteViewer, 
                    ResourceType = RoleResourceType.Site,
                    ResourceId = siteId4,
                    PortfolioId = _portfolioId2,
                    CustomerId = _customerId
                } 
            },
            false);

            Assert.Equal(3, assignments.Count);
            Assert.Equal(_portfolioId1, assignments[2].ResourceId);
            Assert.Equal(RoleResourceType.Portfolio,  assignments[2].ResourceType);
            Assert.Equal(WellKnownRoleIds.PortfolioViewer, assignments[2].RoleId);
            Assert.Equal(siteId3, assignments[0].ResourceId);
            Assert.Equal(siteId4, assignments[1].ResourceId);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_siteadmin_succeeds()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            var result = await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        PortfolioId = _portfolioId1,
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                SiteId = siteId,
                                SiteName = "Bob's Emporium",
                                Role = "Admin"
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false);

            Assert.Single(result);
            Assert.Equal(WellKnownRoleIds.SiteAdmin, result[0].RoleId);
            Assert.Equal(siteId, result[0].ResourceId);
            Assert.Equal(RoleResourceType.Site, result[0].ResourceType);
        }

        [Fact]
        public async Task ManagedUserRequestValidator_Validate_customeradmin_invalid_role()
        {
            var siteId = Guid.NewGuid();

            _siteRepo.Setup( r=> r.Get(siteId)).ReturnsAsync(new Site { Id = siteId, CustomerId = _customerId });

            await Assert.ThrowsAsync<ArgumentException>( async ()=> await _validator.Validate( new CreateManagedUserRequest
            {
                Email = "nobody@nowhere.com",
                IsCustomerAdmin = false,
                Portfolios = new List<ManagedPortfolioDto>
                { 
                    new ManagedPortfolioDto
                    {
                        Role = "",
                        Sites = new List<ManagedSiteDto>
                        {
                            new ManagedSiteDto
                            {
                                Role = "frank",
                                SiteId = siteId
                            }
                        }
                    }
                }
            },
            _customerId,
            new List<RoleAssignmentDto> 
            { 
                new RoleAssignmentDto 
                { 
                    RoleId = WellKnownRoleIds.CustomerAdmin, 
                    ResourceType = RoleResourceType.Customer,
                    ResourceId = _customerId,
                    CustomerId = _customerId
                } 
            },
            new List<RoleAssignmentDto> { },
            false));
        }

        #endregion
    }
}
