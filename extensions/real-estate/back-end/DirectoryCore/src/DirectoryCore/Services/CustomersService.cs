using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Domain;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using DirectoryCore.Services.Auth0;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Services
{
    public interface ICustomersService
    {
        Task<List<Customer>> GetCustomers(bool? active);
        Task<Customer> GetCustomer(Guid customerId);
        Task<IList<Site>> GetSites(Guid customerId, string query);
        Task<Customer> CreateCustomer(CreateCustomerRequest request);

        // <summary>
        // Create the database records needed for a login to work.
        // </summary>
        Task SetupSingleTenantData(CreateCustomerRequest request);

        Task<Customer> UpdateCustomer(Guid customerId, UpdateCustomerRequest request);
        Task<Customer> UpdateCustomerLogo(Guid customerId, byte[] logoImageContent);
        Task<string> GetImpersonateAccessToken(Guid customerId);
        Task<List<Portfolio>> GetPortfolios(Guid customerId, bool includeSites);
        Task<Portfolio> CreatePortfolio(
            Guid customerId,
            string portfolioName,
            PortfolioFeatures features
        );
        Task<bool> DeletePortfolio(Guid customerId, Guid portfolioId);
        Task<Portfolio> UpdatePortfolio(
            Guid customerId,
            Guid portfolioId,
            string portfolioName,
            PortfolioFeatures features
        );
        Task<List<CustomerModelOfInterest>> GetCustomerModelsOfInterest(Guid customerId);
        Task DeleteCustomerModelOfInterest(Guid customerId, Guid id);
        Task<CustomerModelOfInterest> CreateCustomerModelOfInterest(
            Guid customerId,
            CreateCustomerModelOfInterestRequest createCustomerModelOfInterestRequest
        );
        Task<CustomerModelOfInterest> UpdateCustomerModelOfInterest(
            Guid customerId,
            Guid id,
            UpdateCustomerModelOfInterestRequest updateCustomerModelOfInterestRequest
        );
        Task UpdateCustomerModelsOfInterest(
            Guid customerId,
            UpdateCustomerModelsOfInterestRequest updateCustomerModelsOfInterestRequest
        );
    }

    public class CustomersService : ICustomersService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly DirectoryDbContext _directoryContext;
        private readonly IImageHubService _imageHub;
        private readonly IAuth0ManagementService _auth0ManagementService;
        private readonly IAuth0Service _auth0Service;
        private readonly ISitesService _sitesService;
        private readonly SingleTenantOptions _singleTenantOptions;
        private readonly ILogger<CustomersService> _logger;
        private int _modelsOfInterestMaxLimit;
        private const int ModelsOfInterestMaxLimitDefault = 16;
        private static readonly List<CustomerModelOfInterest> DefaultModelsOfInterest =
            new List<CustomerModelOfInterest>
            {
                new CustomerModelOfInterest
                {
                    Id = new Guid("689db4d1-45f7-40c7-b8ee-785f60a94f04"),
                    ModelId = "dtmi:com:willowinc:Asset;1",
                    Name = "Asset",
                    Color = "#DD4FC1",
                    Text = "As"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("301c3fee-35dd-4f74-a0b5-7aeb8afaf2a5"),
                    ModelId = "dtmi:com:willowinc:Building;1",
                    Name = "Building",
                    Color = "#D9D9D9",
                    Text = "Bu"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("81b41e48-1153-42cd-8aa2-96ab994f0a58"),
                    ModelId = "dtmi:com:willowinc:Level;1",
                    Name = "Level",
                    Color = "#E57936",
                    Text = "Lv"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("90caa59a-25d8-43f9-a53d-c789791479e2"),
                    ModelId = "dtmi:com:willowinc:Room;1",
                    Name = "Room",
                    Color = "#55FFD1",
                    Text = "Rm"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("f73f63d9-3f41-4e9e-9585-7e570b99c31d"),
                    ModelId = "dtmi:com:willowinc:System;1",
                    Name = "System",
                    Color = "#78949F",
                    Text = "Sy"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("0b6fc891-46b3-4056-9391-0fec55447e69"),
                    ModelId = "dtmi:com:willowinc:EquipmentGroup;1",
                    Name = "EquipmentGroup",
                    Color = "#417CBF",
                    Text = "Gr"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("cad3731d-1cdf-4817-9d5b-ee006d92183a"),
                    ModelId = "dtmi:com:willowinc:TenantUnit;1",
                    Name = "Tenancy",
                    Color = "#FFC11A",
                    Text = "Te"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("9dd8cddd-7076-4138-bc92-5e6c08203a3e"),
                    ModelId = "dtmi:com:willowinc:Zone;1",
                    Name = "Zone",
                    Color = "#33CA36",
                    Text = "Zn"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("e9fad72b-21c6-42c6-b41c-52cf61ee28ed"),
                    ModelId = "dtmi:com:willowinc:Lease;1",
                    Name = "Lease",
                    Color = "#78949F",
                    Text = "Le"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("94646341-881f-4d34-919a-9b100b9b0673"),
                    ModelId = "dtmi:com:willowinc:Account;1",
                    Name = "Account",
                    Color = "#70DA72",
                    Text = "Ac"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("1f81ff39-1a51-475f-80ee-6046a43cc80f"),
                    ModelId = "dtmi:com:willowinc:Company;1",
                    Name = "Company",
                    Color = "#FD6C76",
                    Text = "Co"
                },
                new CustomerModelOfInterest
                {
                    Id = new Guid("3fe53ba6-8d60-4bfe-840d-cde61501f332"),
                    ModelId = "dtmi:com:willowinc:Land;1",
                    Name = "Land",
                    Color = "#E57936",
                    Text = "La"
                }
            };

        public CustomersService(
            IDateTimeService dateTimeService,
            DirectoryDbContext directoryContext,
            IImageHubService imageHub,
            IAuth0ManagementService auth0ManagementService,
            IAuth0Service auth0Service,
            ISitesService sitesService,
            IOptions<SingleTenantOptions> singleTenantOptions,
            ILogger<CustomersService> logger,
            IConfiguration configuration = null
        )
        {
            _dateTimeService = dateTimeService;
            _directoryContext = directoryContext;
            _imageHub = imageHub;
            _auth0ManagementService = auth0ManagementService;
            _auth0Service = auth0Service;
            _sitesService = sitesService;
            _singleTenantOptions = singleTenantOptions.Value;
            _logger = logger;
            if (configuration != null)
            {
                _modelsOfInterestMaxLimit = configuration.GetValue<int>(
                    "ModelsOfInterestMaxLimit",
                    ModelsOfInterestMaxLimitDefault
                );
            }
        }

        public async Task<List<Customer>> GetCustomers(bool? active)
        {
            IQueryable<CustomerEntity> customerEntities = _directoryContext.Customers;

            if (active.HasValue)
            {
                customerEntities = customerEntities.Where(
                    c => (c.Status == CustomerStatus.Active) == active.Value
                );
            }

            return CustomerEntity.MapTo(await customerEntities.ToListAsync());
        }

        public async Task<Customer> GetCustomer(Guid customerId)
        {
            var result = await _directoryContext.Customers.FirstOrDefaultAsync(
                c => c.Id == customerId
            );

            return CustomerEntity.MapTo(result);
        }

        public async Task<IList<Site>> GetSites(Guid customerId, string query)
        {
            IList<Site> result;
            if (!string.IsNullOrWhiteSpace(query))
            {
                result = await _sitesService.GetSitesByCustomer(customerId);
                result = result.Where(s => s.Name.Contains(query)).ToList();

                return result;
            }

            result = await _sitesService.GetSitesByCustomer(customerId);

            return result;
        }

        private async Task ThrowIfSigmaConnectionIdInUse(string sigmaConnectionId, Guid? customerId)
        {
            if (!string.IsNullOrEmpty(sigmaConnectionId))
            {
                var inUseByOther = false;

                if (customerId.HasValue)
                {
                    inUseByOther = await _directoryContext.Customers.AnyAsync(
                        x => x.SigmaConnectionId == sigmaConnectionId && x.Id != customerId
                    );
                }
                else
                {
                    inUseByOther = await _directoryContext.Customers.AnyAsync(
                        x => x.SigmaConnectionId == sigmaConnectionId
                    );
                }

                if (inUseByOther)
                {
                    throw new BadRequestException(
                        $"SigmaConnectionId {sigmaConnectionId} is already in use."
                    );
                }
            }
        }

        /// <summary>
        /// Create a Customers record, a Users record with an associated auth0 user,
        /// and an Assignments record assigning the user as a customer admin.
        /// </summary>
        public async Task<Customer> CreateCustomer(CreateCustomerRequest request)
        {
            await ThrowIfSigmaConnectionIdInUse(request.SigmaConnectionId, null);

            var customerEntity = new CustomerEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Address1 = string.Empty,
                Address2 = string.Empty,
                Suburb = string.Empty,
                Postcode = string.Empty,
                Country = request.Country,
                State = string.Empty,
                LogoId = null,
                Status = CustomerStatus.Active,
                AccountExternalId = request.AccountExternalId ?? string.Empty,
                SigmaConnectionId = request.SigmaConnectionId,
                FeaturesJson = string.Empty
            };

            _directoryContext.Customers.Add(customerEntity);
            await _directoryContext.SaveChangesAsync();

            await CreateCustomerSupportAccount(customerEntity.Id);

            return CustomerEntity.MapTo(customerEntity);
        }

        public async Task SetupSingleTenantData(CreateCustomerRequest request)
        {
            if (request.Id == null)
            {
                throw new ArgumentNullException(
                    "Customer Id is required for SetupSingleTenantData"
                );
            }

            var customerId = request.Id.Value;

            // Note that we check for the existence of the objects before we create them,
            // even though the db's integrity constraints will prevent creation of duplicates.
            // It's possible (and arguably better) to just try to create them and catch the
            // integrity constraint exceptions, but I couldn't figure out how to do this
            // without having EF log the exceptions, which would be confusing for someone
            // looking at the logs.
            if (!await _directoryContext.Customers.AnyAsync(x => x.Id == customerId))
            {
                var customerEntity = new CustomerEntity
                {
                    Id = customerId,
                    Name = request.Name,
                    Address1 = string.Empty,
                    Address2 = string.Empty,
                    Suburb = string.Empty,
                    Postcode = string.Empty,
                    Country = request.Country,
                    State = string.Empty,
                    LogoId = null,
                    Status = CustomerStatus.Active,
                    AccountExternalId = request.AccountExternalId ?? string.Empty,
                    SigmaConnectionId = request.SigmaConnectionId,
                    FeaturesJson = string.Empty,
                    ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                        DefaultModelsOfInterest
                    )
                };

                _directoryContext.Customers.Add(customerEntity);
                await _directoryContext.SaveChangesAsync();
                _logger.LogInformation("SetupSingleTenantData: Created Customer");
            }
            else
            {
                _logger.LogInformation(
                    "SetupSingleTenantData: Customer already setup; not creating"
                );
            }

            var userId = _singleTenantOptions.CustomerUserIdForGroupUser;
            if (!await _directoryContext.Users.AnyAsync(x => x.Id == userId))
            {
                var user = new UserEntity
                {
                    Id = userId,
                    CustomerId = customerId,
                    // For hybrid instances, we will have one customer user per customer in the MT databases,
                    // so make sure their email addresses are unique. See
                    // https://willow.atlassian.net/wiki/spaces/~918887516/pages/2501771305/User+management+proposal
                    Email = $"admin-{userId}@willowinc.com",
                    EmailConfirmed = true,
                    EmailConfirmationToken = string.Empty,
                    EmailConfirmationTokenExpiry = _dateTimeService.UtcNow,
                    FirstName = "Admin",
                    LastName = "Willow",
                    AvatarId = null,
                    CreatedDate = _dateTimeService.UtcNow,
                    Initials = "A W",
                    Auth0UserId = string.Empty,
                    Mobile = string.Empty,
                    Status = UserStatus.Active,
                };
                _directoryContext.Users.Add(user);
                await _directoryContext.SaveChangesAsync();
                _logger.LogInformation("SetupSingleTenantData: Created User");
            }
            else
            {
                _logger.LogInformation("SetupSingleTenantData: User already setup; not creating");
            }

            if (!await _directoryContext.Assignments.AnyAsync(x => x.PrincipalId == userId))
            {
                var customerAdminRoleAssignment = new AssignmentEntity
                {
                    PrincipalId = userId,
                    PrincipalType = PrincipalType.User,
                    RoleId = WellKnownRoleIds.CustomerAdmin,
                    ResourceId = customerId,
                    ResourceType = RoleResourceType.Customer
                };
                _directoryContext.Assignments.Add(customerAdminRoleAssignment);
                await _directoryContext.SaveChangesAsync();
                _logger.LogInformation("SetupSingleTenantData: Created Assignment");
            }
            else
            {
                _logger.LogInformation(
                    "SetupSingleTenantData: Assignment already setup; not creating"
                );
            }
        }

        private async Task CreateCustomerSupportAccount(Guid customerId)
        {
            var supportAccount = new UserEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Email = WellKnownUsers.CustomerSupport.Email,
                EmailConfirmed = true,
                EmailConfirmationToken = string.Empty,
                EmailConfirmationTokenExpiry = _dateTimeService.UtcNow,
                FirstName = WellKnownUsers.CustomerSupport.FirstName,
                LastName = WellKnownUsers.CustomerSupport.LastName,
                AvatarId = null,
                CreatedDate = _dateTimeService.UtcNow,
                Initials = WellKnownUsers.CustomerSupport.Initials,
                Auth0UserId = string.Empty,
                Mobile = string.Empty,
                Status = UserStatus.Active,
            };

            var randomPassword = Guid.NewGuid().ToString();
            var auth0UserId = await _auth0ManagementService.CreateUser(
                supportAccount.Id,
                WellKnownUsers.CustomerSupport.UserName(customerId),
                supportAccount.FirstName,
                supportAccount.LastName,
                randomPassword,
                UserTypeNames.CustomerUser
            );

            supportAccount.Auth0UserId = auth0UserId;

            var customerAdminRoleAssignment = new AssignmentEntity
            {
                PrincipalId = supportAccount.Id,
                PrincipalType = PrincipalType.User,
                RoleId = WellKnownRoleIds.CustomerAdmin,
                ResourceId = customerId,
                ResourceType = RoleResourceType.Customer
            };

            _directoryContext.Users.Add(supportAccount);
            _directoryContext.Assignments.Add(customerAdminRoleAssignment);

            await _directoryContext.SaveChangesAsync();
        }

        public async Task<Customer> UpdateCustomer(Guid customerId, UpdateCustomerRequest request)
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customerEntity == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            if (!string.IsNullOrEmpty(request.SigmaConnectionId))
            {
                await ThrowIfSigmaConnectionIdInUse(request.SigmaConnectionId, customerId);

                customerEntity.SigmaConnectionId = request.SigmaConnectionId;
            }

            await _directoryContext.SaveChangesAsync();

            return CustomerEntity.MapTo(customerEntity);
        }

        public async Task<Customer> UpdateCustomerLogo(Guid customerId, byte[] logoImageContent)
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customerEntity == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            var imageId = await _imageHub.CreateCustomerLogo(customerId, logoImageContent);
            customerEntity.LogoId = imageId;

            await _directoryContext.SaveChangesAsync();

            return CustomerEntity.MapTo(customerEntity);
        }

        public async Task<string> GetImpersonateAccessToken(Guid customerId)
        {
            var supportAccount = await _directoryContext.Users.FirstOrDefaultAsync(
                u => u.CustomerId == customerId && u.Email == WellKnownUsers.CustomerSupport.Email
            );
            if (supportAccount == null)
            {
                throw new BadRequestException(
                    $"The customer ({customerId}) does not have support user."
                );
            }
            if (supportAccount.Status != UserStatus.Active)
            {
                throw new BadRequestException(
                    $"The support user in customer ({customerId}) is not Active. Status: {supportAccount.Status}"
                );
            }

            var randomPassword = Guid.NewGuid().ToString();
            await _auth0ManagementService.ChangeUserPassword(
                supportAccount.Auth0UserId,
                randomPassword
            );

            var tokenResponse = await _auth0Service.GetAccessTokenByPassword(
                WellKnownUsers.CustomerSupport.UserName(customerId),
                randomPassword
            );

            return tokenResponse.AccessToken;
        }

        public async Task<List<Portfolio>> GetPortfolios(Guid customerId, bool includeSites)
        {
            var portfolios = await _directoryContext
                .Portfolios.Where(x => x.CustomerId == customerId)
                .ToListAsync();
            var result = PortfolioEntity.MapTo(portfolios);
            if (includeSites)
            {
                var siteEntities = await _sitesService.GetSitesByCustomer(customerId);
                var sitesPerPortfolio = siteEntities
                    .GroupBy(x => x.PortfolioId)
                    .ToDictionary(x => x.Key);
                foreach (var portfolio in result)
                {
                    if (sitesPerPortfolio.TryGetValue(portfolio.Id, out var sites))
                    {
                        portfolio.Sites = sites.ToList();
                    }
                }
            }

            return result;
        }

        public async Task<Portfolio> CreatePortfolio(
            Guid customerId,
            string portfolioName,
            PortfolioFeatures features
        )
        {
            var portfolioEntity = new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                Name = portfolioName,
                CustomerId = customerId,
                FeaturesJson = JsonSerializerExtensions.Serialize(features)
            };
            _directoryContext.Portfolios.Add(portfolioEntity);
            await _directoryContext.SaveChangesAsync();

            return PortfolioEntity.MapTo(portfolioEntity);
        }

        public async Task<bool> DeletePortfolio(Guid customerId, Guid portfolioId)
        {
            var portfolio = await _directoryContext
                .Portfolios.AsTracking()
                .Where(x => x.CustomerId == customerId && x.Id == portfolioId)
                .FirstOrDefaultAsync();
            if (portfolio == null)
            {
                return false;
            }

            _directoryContext.Portfolios.Remove(portfolio);
            await _directoryContext.SaveChangesAsync();
            return true;
        }

        public async Task<Portfolio> UpdatePortfolio(
            Guid customerId,
            Guid portfolioId,
            string portfolioName,
            PortfolioFeatures features
        )
        {
            var portfolio = await _directoryContext
                .Portfolios.AsTracking()
                .Where(x => x.CustomerId == customerId && x.Id == portfolioId)
                .FirstOrDefaultAsync();
            if (portfolio == null)
            {
                throw new ResourceNotFoundException("portfolio", portfolioId);
            }
            portfolio.Name = portfolioName;
            portfolio.FeaturesJson = JsonSerializerExtensions.Serialize(features);
            _directoryContext.Portfolios.Update(portfolio);
            await _directoryContext.SaveChangesAsync();

            return PortfolioEntity.MapTo(portfolio);
        }

        public async Task<List<CustomerModelOfInterest>> GetCustomerModelsOfInterest(
            Guid customerId
        )
        {
            var customer = await _directoryContext.Customers.FirstOrDefaultAsync(
                c => c.Id == customerId
            );

            if (customer == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            return CustomerEntity.MapCustomerModelsOfInterest(customer.ModelsOfInterestJson);
        }

        public async Task DeleteCustomerModelOfInterest(Guid customerId, Guid id)
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customerEntity == null)
            {
                throw new BadRequestException("CustomerId is invalid");
            }

            var customerModelsOfInterest = CustomerEntity.MapCustomerModelsOfInterest(
                customerEntity.ModelsOfInterestJson
            );
            var customerModelOfInterest = customerModelsOfInterest?.FirstOrDefault(x => x.Id == id);
            if (customerModelOfInterest == null)
            {
                throw new BadRequestException("ModelId is invalid");
            }

            customerModelsOfInterest.Remove(customerModelOfInterest);
            customerEntity.ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                customerModelsOfInterest
            );
            customerEntity.ModelsOfInterestETag = Guid.NewGuid();

            _directoryContext.Customers.Update(customerEntity);

            await _directoryContext.SaveChangesAsync();
        }

        public async Task<CustomerModelOfInterest> CreateCustomerModelOfInterest(
            Guid customerId,
            CreateCustomerModelOfInterestRequest createCustomerModelOfInterestRequest
        )
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customerEntity == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            var modelOfInterests = CustomerEntity.MapCustomerModelsOfInterest(
                customerEntity?.ModelsOfInterestJson
            );

            if (modelOfInterests != null)
            {
                if (modelOfInterests.Count == _modelsOfInterestMaxLimit)
                {
                    throw new BadRequestException(
                        $"Models of interest has already reached the maximum limit of {_modelsOfInterestMaxLimit}."
                    );
                }

                if (
                    modelOfInterests.Any(
                        x => x.ModelId == createCustomerModelOfInterestRequest.ModelId
                    )
                )
                {
                    throw new BadRequestException("Model already exists.");
                }
            }
            else
            {
                modelOfInterests = new List<CustomerModelOfInterest>();
            }

            var newModelOfInterest = new CustomerModelOfInterest
            {
                Id = Guid.NewGuid(),
                ModelId = createCustomerModelOfInterestRequest.ModelId,
                Name = createCustomerModelOfInterestRequest.Name,
                Color = createCustomerModelOfInterestRequest.Color,
                Text = createCustomerModelOfInterestRequest.Text,
                Icon = createCustomerModelOfInterestRequest.Icon
            };
            modelOfInterests.Add(newModelOfInterest);

            customerEntity.ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                modelOfInterests
            );
            customerEntity.ModelsOfInterestETag = Guid.NewGuid();

            _directoryContext.Customers.Update(customerEntity);
            await _directoryContext.SaveChangesAsync();

            return newModelOfInterest;
        }

        public async Task<CustomerModelOfInterest> UpdateCustomerModelOfInterest(
            Guid customerId,
            Guid id,
            UpdateCustomerModelOfInterestRequest updateCustomerModelOfInterestRequest
        )
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customerEntity == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            var modelsOfInterest = CustomerEntity.MapCustomerModelsOfInterest(
                customerEntity.ModelsOfInterestJson
            );
            var modelOfInterest = modelsOfInterest?.FirstOrDefault(x => x.Id == id);
            if (modelOfInterest == null)
            {
                throw new BadRequestException("Model not found in the list.");
            }

            if (
                modelsOfInterest
                    .Except(modelsOfInterest.Where(x => x.Id == id))
                    .Any(x => x.ModelId == updateCustomerModelOfInterestRequest.ModelId)
            )
            {
                throw new BadRequestException("Model already exists.");
            }

            var index = modelsOfInterest.IndexOf(modelOfInterest);
            if (index != -1)
            {
                modelOfInterest.ModelId = updateCustomerModelOfInterestRequest.ModelId;
                modelOfInterest.Name = updateCustomerModelOfInterestRequest.Name;
                modelOfInterest.Color = updateCustomerModelOfInterestRequest.Color;
                modelOfInterest.Text = updateCustomerModelOfInterestRequest.Text;
                modelOfInterest.Icon = updateCustomerModelOfInterestRequest.Icon;
                modelsOfInterest[index] = modelOfInterest;
            }

            customerEntity.ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                modelsOfInterest
            );
            customerEntity.ModelsOfInterestETag = Guid.NewGuid();

            _directoryContext.Customers.Update(customerEntity);
            await _directoryContext.SaveChangesAsync();

            return modelOfInterest;
        }

        public async Task UpdateCustomerModelsOfInterest(
            Guid customerId,
            UpdateCustomerModelsOfInterestRequest updateCustomerModelsOfInterestRequest
        )
        {
            var customerEntity = await _directoryContext
                .Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customerEntity == null)
            {
                throw new ResourceNotFoundException("customer", customerId);
            }

            customerEntity.ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                CustomerModelOfInterest.MapFrom(
                    updateCustomerModelsOfInterestRequest.ModelsOfInterest
                )
            );
            customerEntity.ModelsOfInterestETag = Guid.NewGuid();

            _directoryContext.Customers.Update(customerEntity);
            await _directoryContext.SaveChangesAsync();
        }
    }
}
