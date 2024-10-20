using Authorization.Common.Enums;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Services.Hosted;
using Authorization.TwinPlatform.Services.Hosted.Request;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Applications.Item.AddPassword;
using Microsoft.Graph.Applications.Item.RemovePassword;
using Microsoft.Graph.Models;
using System.Collections.Concurrent;
using AppRegistration = Microsoft.Graph.Models.Application;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Service for interacting with Microsoft Graph API
/// </summary>
public class GraphApplicationService : IGraphApplicationService
{
    private readonly ILogger<GraphApplicationService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IBackgroundQueueSender<GroupMembershipCacheRefreshRequest> _sender;

    public GraphApplicationService(ILogger<GraphApplicationService> logger, IMemoryCache memoryCache, IBackgroundQueueSender<GroupMembershipCacheRefreshRequest> sender)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _sender = sender;
    }

    /// <summary>
    /// Get Graph Users by Ids
    /// </summary>
    /// <param name="service">Graph Application Client Service.</param>
    /// <param name="ids">Array of User Ids.</param>
    /// <returns>List of UserModel.</returns>
    public async Task<List<UserModel>> GetUsersByIds(IGraphApplicationClientService service, string[] ids)
    {
        List<UserModel> result = [];
        if (ids is null || ids.Length == 0) return result;

        // make sure the ids are distinct
        ids = ids.Distinct().ToArray();

        _logger.LogInformation("Getting graph user by Ids: {Ids}", string.Join(',', ids));

        static UserModel mapUserToUserModel(User user)
        {
            return new UserModel()
            {
                Id = Guid.Parse(user.Id!),
                FirstName = user.GivenName ?? string.Empty,
                LastName = user.Surname ?? string.Empty,
                Status = user.AccountEnabled == true ? UserStatus.Active : UserStatus.Inactive,
                Email = user.UserPrincipalName ?? string.Empty,
                CreatedDate = user.CreatedDateTime.GetValueOrDefault()
            };
        }

        try
        {
            var graphClient = service.GetGraphServiceClient();

            // check if the id is already cached
            foreach (var id in ids)
            {
                if (_memoryCache.TryGetValue($"User_{service.GraphConfiguration.Type}_Id_{id}", out UserModel? cachedUser))
                {
                    result.Add(cachedUser!);
                    continue;
                }

                var userCollection = await graphClient.Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id,givenName,surname,accountEnabled,userPrincipalName,createdDateTime"];
                    requestConfiguration.QueryParameters.Top = 1;
                    requestConfiguration.QueryParameters.Filter = $"id eq '{id}'";
                });

                var retrievedUser = userCollection?.Value?.SingleOrDefault();

                if (retrievedUser != null)
                {
                    var model = mapUserToUserModel(retrievedUser);

                    // Add to the cache for future retrieval
                    _memoryCache.Set($"User_{service.GraphConfiguration.Type}_Id_{id}", model, DateTimeOffset.MaxValue);

                    // Add to the result
                    result.Add(model);
                }

            }

        }
        catch (Exception ex)
        {
            service.HealthCheckInstance.Current = HealthCheckAD.FailingCalls;
            _logger.LogError(ex, "Error occurred while retrieving AD User Object Ids: {Ids}", string.Join(',', ids));
        }
        return result;
    }

    /// <summary>
    /// Get AD User name identifier based on user mail address
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="mail">Mail address form the AD User object.</param>
    /// <returns>Name Identifier string</returns>
    public async Task<string?> GetUserIdByEmailAddress(IGraphApplicationClientService graphClientService, string mail)
    {
        ArgumentException.ThrowIfNullOrEmpty(mail);

        _logger.LogInformation("Getting name Identifier for mail Address : {mail}", mail);

        try
        {
            return await _memoryCache.GetOrCreateAsync($"User_{graphClientService.GraphConfiguration.Type}_email_{mail}", async (cacheEntry) =>
            {
                var graphClient = graphClientService.GetGraphServiceClient();
                var graphConfig = graphClientService.GraphConfiguration;

                var userCollection = await graphClient.Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["Id"];
                    requestConfiguration.QueryParameters.Top = 1;
                    if (graphConfig.Type == ADType.AzureB2C)
                    {
                        requestConfiguration.QueryParameters.Filter = $"userType eq 'Member' and otherMails/any(i:i eq '{mail}')";
                    }
                    else if (graphConfig.Type == ADType.AzureAD)
                    {
                        requestConfiguration.QueryParameters.Filter = $"userType eq 'Member' and userPrincipalName eq '{mail}'";
                    }
                });

                var user = userCollection?.Value?.SingleOrDefault();

                graphClientService.HealthCheckInstance.Current = HealthCheckAD.Healthy;

                if (user is null)
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // cache for 10 min, before again querying for a non existing user
                    _logger.LogWarning("Unable to find AD {ADType} user object with mail address {mail}", graphConfig.Type.ToString(), mail);
                }
                else
                {
                    // We will indefinitely cache the user Id if found
                    cacheEntry.Priority = CacheItemPriority.NeverRemove;
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.MaxValue;
                }

                return user?.Id;
            });
        }
        catch (Exception ex)
        {
            graphClientService.HealthCheckInstance.Current = HealthCheckAD.FailingCalls;
            _logger.LogError(ex, "Error occurred while retrieving AD User Object for mail address {mail}", mail);
        }
        return null;
    }

    /// <summary>
    /// Get Group Object Id by Display Name
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupName">Display Name of the group.</param>
    /// <returns>Object Id of the group.</returns>
    public async Task<string?> GetGroupIdByName(IGraphApplicationClientService graphClientService, string groupName)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);

        _logger.LogInformation("Getting object Id for group name : {GroupName}", groupName);

        try
        {
            return await _memoryCache.GetOrCreateAsync($"{graphClientService.GraphConfiguration.Type}_{groupName}_Id", async (cacheEntry) =>
            {
                var graphClient = graphClientService.GetGraphServiceClient();

                var groupCollection = await graphClient.Groups.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["Id"];
                    requestConfiguration.QueryParameters.Top = 1;
                    requestConfiguration.QueryParameters.Filter = $"mailEnabled eq false and securityEnabled eq true and displayName eq '{groupName}'";
                });

                var group = groupCollection?.Value?.SingleOrDefault();

                graphClientService.HealthCheckInstance.Current = HealthCheckAD.Healthy;

                if (group is null)
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // cache for 10 min, before query for a non existing group
                    _logger.LogWarning("Unable to find AD {ADType} Group object with name {mail}", graphClientService.GraphConfiguration.Type.ToString(), groupName);
                }
                else
                {
                    // We will indefinitely cache the group Id if found
                    cacheEntry.Priority = CacheItemPriority.NeverRemove;
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.MaxValue;
                }
                return group?.Id;
            });
        }
        catch (Exception ex)
        {
            graphClientService.HealthCheckInstance.Current = HealthCheckAD.FailingCalls;
            _logger.LogError(ex, "Error occurred while retrieving AD group Object for name {Name}", groupName);
        }
        return null;
    }

    /// <summary>
    /// Get group user members from graph service.
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupId">Object Id of the group.</param>
    /// <param name="includeTransitiveMembers">True to include transitive members; false to return only direct members.</param>
    /// <param name="useCache">Retrieves value from the cache if exist; false to get fresh data from the data store. Default is true.</param>
    /// <returns>Enumerable of group member user Id.</returns>
    public async Task<IEnumerable<string>> GetGroupMemberships(IGraphApplicationClientService graphClientService, string groupId, bool includeTransitiveMembers = true, bool useCache = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupId);

        _logger.LogInformation("Getting group membership for groupId : {groupId}", groupId);

        var graphClient = graphClientService.GetGraphServiceClient();
        var cacheKeyName = includeTransitiveMembers ? $"{graphClientService.GraphConfiguration.Type}_{groupId}_transitive" :
                                                      $"{graphClientService.GraphConfiguration.Type}_{groupId}";

        try
        {
            IEnumerable<string>? result = null;
            if (useCache && _memoryCache.TryGetValue<IEnumerable<string>>(cacheKeyName, out result))
            {
                return result!;
            }

            DirectoryObjectCollectionResponse? groupMemberships = null;
            int top = 100;
            do
            {
                if (groupMemberships?.OdataNextLink is null)
                {
                    if (includeTransitiveMembers)
                    {
                        groupMemberships = await graphClient.Groups[groupId].TransitiveMembers.GetAsync((req) =>
                        {
                            req.QueryParameters.Select = ["Id"];
                            req.QueryParameters.Top = top;
                        });
                    }
                    else
                    {
                        groupMemberships = await graphClient.Groups[groupId].MemberOf.GetAsync((req) =>
                        {
                            req.QueryParameters.Select = ["Id"];
                            req.QueryParameters.Top = top;
                        });
                    }
                }
                else
                {
                    groupMemberships = await graphClient.DirectoryObjects.WithUrl(groupMemberships.OdataNextLink).GetAsync((req) =>
                    {
                        req.QueryParameters.Select = ["Id"];
                        req.QueryParameters.Top = top;
                    });
                }

                if (groupMemberships is null || groupMemberships.Value is null || groupMemberships.Value.Count == 0)
                {
                    result ??= Enumerable.Empty<string>();
                }
                else
                {
                    var currResult = groupMemberships.Value.OfType<User>().Select(s => s.Id!);
                    result = (result?.Union(currResult) ?? currResult).ToList();
                }

            } while (groupMemberships?.OdataNextLink is not null);

            _memoryCache.Set(cacheKeyName, result, graphClientService.GraphConfiguration.CacheExpiration);
            return result;
        }
        catch (Exception ex)
        {
            graphClientService.HealthCheckInstance.Current = HealthCheckAD.FailingCalls;
            _logger.LogError(ex, "Error while fetching group with Id:{GroupId} memberships.", groupId);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Filter input group names the user is a member of.
    /// </summary>
    /// <param name="graphClientService">Instance of AD Client connection service.</param>
    /// <param name="groupNames">List of group names to evaluate.</param>
    /// <param name="email">Email of the user.</param>
    /// <param name="includeTransitiveMembership">True to include transitive members; false to return only direct members.</param>
    /// <returns>Filtered Enumerable of group names.</returns>
    public async Task<IEnumerable<string>> FilterGroupsOfUserByEmailAsync(IGraphApplicationClientService graphClientService, IEnumerable<string> groupNames, string email, bool includeTransitiveMembership = true)
    {
        var userId = await GetUserIdByEmailAddress(graphClientService, email);
        if (string.IsNullOrWhiteSpace(userId))
            return Enumerable.Empty<string>();

        try
        {
            ConcurrentBag<string> result = [];
            async Task AppendGroupIfMember(string currGroupName)
            {
                // Get the relevant group Id
                var currGroupId = await GetGroupIdByName(graphClientService, currGroupName); // Group Id for group name cached indefinitely

                // Skip if you can't find the group in the AD
                if (currGroupId is null) return;

                // Request a lazy delayed cache for group membership
                if (_sender.GetStatus())
                {
                    _sender.Enqueue(new GroupMembershipCacheRefreshRequest(graphClientService, currGroupId, includeTransitiveMembership));
                }
                var currGroupMemberships = await GetGroupMemberships(graphClientService, currGroupId, includeTransitiveMembership);

                // check if the group contains the User Id
                if (currGroupMemberships is not null && currGroupMemberships.Any(memberId => memberId == userId))
                {
                    result.Add(currGroupName);
                }
            }

            var groupTasks = groupNames.Select(g => AppendGroupIfMember(g));
            await Task.WhenAll(groupTasks);

            graphClientService.HealthCheckInstance.Current = HealthCheckAD.Healthy;
            return result;
        }
        catch (Exception)
        {
            graphClientService.HealthCheckInstance.Current = HealthCheckAD.FailingCalls;
            throw;
        }

    }

    /// <summary>
    /// Adds new App Registration to the Tenant
    /// </summary>
    /// <param name="graphClientService">Graph Application Client.</param>
    /// <param name="appRegistrationName">Name of the App Registration.</param>
    /// <param name="description">Description for the App Registration.</param>
    /// <param name="appRegistrationSetting">Additional Client App Registration Settings.</param>
    /// <returns></returns>
    public async Task<string> AddAppRegistrationAsync(IGraphApplicationClientService graphClientService,
        string appRegistrationName,
        string description,
        ClientAppRegistrationSettings appRegistrationSetting)
    {
        try
        {
            var client = graphClientService.GetGraphServiceClient();

            var createdApplication = await client.Applications.PostAsync(new AppRegistration()
            {
                DisplayName = appRegistrationName, // App Registration Name should of format: {FullCustomerInstanceName from Willow Context}-{unique App Name}
                SignInAudience = appRegistrationSetting.SignInAudience,
                Description = description,
                Web = new WebApplication()
                {
                    ImplicitGrantSettings = new ImplicitGrantSettings()
                    {
                        EnableAccessTokenIssuance = true,
                        EnableIdTokenIssuance = true,
                    }
                }
            });

            return createdApplication?.AppId!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client app registration: {Name}", appRegistrationName);
            throw;
        }

    }

    /// <summary>
    /// Get Password Credentials By Application Client Ids
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client</param>
    /// <param name="clientIds">List of Client Ids.</param>
    /// <returns>Dictionary of Client Id as key  list of password credentials as value. </returns>
    public async Task<Dictionary<string, List<ClientAppPasswordCredential>>> GetPasswordCredentialsByIds(
        IGraphApplicationClientService graphApplicationClient,
        IEnumerable<string> clientIds)
    {
        try
        {
            var client = graphApplicationClient.GetGraphServiceClient();
            if (clientIds?.Any() != true)
            {
                return [];
            }

            var result = await client.Applications.GetAsync((req) =>
            {
                req.QueryParameters.Filter = string.Join(" or ", clientIds.Select(s => $"appId eq '{s}'"));
                req.QueryParameters.Select = ["appId", "passwordCredentials"];
            });

            if (result is null || result.Value is null)
            {
                return [];
            }

            Dictionary<string, List<ClientAppPasswordCredential>> response = [];
            foreach (var directoryObject in result.Value)
            {
                if (directoryObject is AppRegistration registration)
                {
                    response.Add(registration.AppId!, registration.PasswordCredentials?.Select(s => MapPasswordCredentials(s))?.ToList() ?? []);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password credentials.");
            throw;
        }
    }

    /// <summary>
    /// Adds Password Credential to the App Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Application (Client) Id of the App Registration.</param>
    /// <param name="secretName">Name of the secret to create.</param>
    /// <param name="expiryTimeSpan">Requested expiration datetime offset.</param>
    /// <returns>ClientAppPasswordCredential.</returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<ClientAppPasswordCredential> AddPasswordCredentials(IGraphApplicationClientService graphApplicationClient, string clientId, string secretName, TimeSpan expiryTimeSpan)
    {
        try
        {
            var client = graphApplicationClient.GetGraphServiceClient();
            var targetApplication = await client.ApplicationsWithAppId(clientId).GetAsync();

            var requestBody = new AddPasswordPostRequestBody()
            {
                PasswordCredential = new PasswordCredential()
                {
                    DisplayName = secretName,
                    StartDateTime = DateTimeOffset.Now,
                    EndDateTime = DateTimeOffset.Now.Add(expiryTimeSpan),
                }
            };

            var passwordCredentials = await client.Applications[targetApplication!.Id].AddPassword.PostAsync(requestBody);
            if (passwordCredentials == null)
                throw new InvalidDataException();
            else
            {
                return MapPasswordCredentials(passwordCredentials);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding password credential: {FullSecretName}", secretName);
            throw;
        }
    }

    /// <summary>
    /// Remove and delete all password credentials from the App Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Application (Client) Id.</param>
    /// <returns>Awaitable task.</returns>
    public async Task ClearPasswordCredentials(IGraphApplicationClientService graphApplicationClient, string clientId)
    {
        try
        {
            var client = graphApplicationClient.GetGraphServiceClient();
            var targetApplication = await client.ApplicationsWithAppId(clientId).GetAsync();
            if (targetApplication != null)
            {

                foreach (var cred in targetApplication.PasswordCredentials!)
                {
                    RemovePasswordPostRequestBody body = new()
                    {
                        KeyId = cred.KeyId,
                    };
                    await client.Applications[targetApplication.Id].RemovePassword.PostAsync(body);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting password credentials: {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// Delete Application Registration.
    /// </summary>
    /// <param name="graphApplicationClient">Graph Application Client.</param>
    /// <param name="clientId">Client Id of the Application.</param>
    /// <returns>Awaitable Task.</returns>
    public async Task DeleteAppRegistration(IGraphApplicationClientService graphApplicationClient, string clientId)
    {
        try
        {
            // Get the Client
            var client = graphApplicationClient.GetGraphServiceClient();

            // Delete the App Registration
            await client.ApplicationsWithAppId(clientId).DeleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting the Application Registration {ClientId}", clientId);
            throw;
        }
    }

    private static ClientAppPasswordCredential MapPasswordCredentials(PasswordCredential passwordCredentials)
    {
        return new ClientAppPasswordCredential(passwordCredentials.DisplayName ?? string.Empty,
                     passwordCredentials.SecretText ?? string.Empty,
                     passwordCredentials.StartDateTime.GetValueOrDefault(),
                     passwordCredentials.EndDateTime.GetValueOrDefault());
    }
}
