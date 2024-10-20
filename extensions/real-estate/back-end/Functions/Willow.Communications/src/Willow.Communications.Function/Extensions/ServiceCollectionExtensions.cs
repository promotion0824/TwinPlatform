using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.Api.AzureStorage;
using Willow.Common;
using Willow.Data;
using Willow.Communications.Function.Functions;
using Willow.Communications.Function.Models;
using Willow.Communications.Function.Resolvers;
using Willow.Communications.Function.Services;
using Willow.Communications.Resolvers.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Communications.Function.Cache;
using Willow.Data.Rest;

namespace Willow.Communications.Function.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommSvc(this IServiceCollection services)
        {
            services.AddMemoryCache()
                    .AddHttpClient()
                    .AddAuth0()
                    .SetHttpClient<CommunicationsService>(ApiServiceNames.DirectoryCore)
                    .AddCachedRestRepository<Guid, Customer>(ApiServiceNames.DirectoryCore, (Guid id)=> $"customers/{id}", cacheDurationInHours: 4)
                    .AddSingleton<IRecipientResolver>( p=>
                    {
                        var userRepo = p.CreateRestRepository<UserRequest, User>(ApiServiceNames.DirectoryCore, (UserRequest request)=> $"users/{request.UserId}" + (request.UserType.HasValue ? $"?userType={(int)request.UserType}" : ""), null);

                        return new RecipientResolver(userRepo);
                    })
                    .AddSingleton<IPushNotificationService>(p => 
                    {
                        var config = p.GetRequiredService<IConfiguration>();
                        var logger = p.GetRequiredService<ILogger<PushNotificationService>>();

                        return new PushNotificationService(config.Get("PushNotificationConnectionString", logger), config.Get("PushNotificationHubPath", logger));
                    })
                    .AddSingleton<ICommunicationsService>( p=> 
                    {
                        var logger     = p.GetRequiredService<ILogger<CommunicationsService>>();

                        try
                        { 
                            var config     = p.GetRequiredService<IConfiguration>();
                            var pushNotify = p.GetRequiredService<IPushNotificationService>();
                            var sendGrid   = new SendGridEmailService(config.Get("SendGridApiKey", logger), 
                                                                      config.Get("EmailFromAddress", logger), 
                                                                      config.Get("EmailFromName", logger));
                            var hubs       = new Dictionary<string, INotificationHub> 
                            { 
                               { "email", sendGrid }
                              ,{ "pushnotification", pushNotify } 
                            };
                            var templateStore = p.CreateBlobStore<CommunicationsServiceFunction>(new BlobStorageConfig { AccountName   = config.Get("ContentStorageAccountName", logger), 
                                                                                                                         ContainerName = config.Get("ContentStorageContainerName", logger), 
                                                                                                                         AccountKey    = config.Get("ContentStorageAccountKey", logger, false)
                                                                                                                       },
                                                                                                                       "");
                            var recipientResolver = p.GetRequiredService<IRecipientResolver>(); 
                            var customerRepo = p.GetRequiredService<IReadRepository<Guid, Customer>>(); 

                            return new CommunicationsService(hubs, templateStore, recipientResolver, customerRepo, logger);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(new Exception("Unable to create communication service", ex), ex.Message);
                            throw;
                        }
                    });  

            return services;   
        }

        public static IServiceCollection SetHttpClient<T>(this IServiceCollection services, string name)
        {
            return services.SetHttpClient<T>(name, "", hasNamedM2MAuth: false);
        }

        public static IServiceCollection SetHttpClient<T>(this IServiceCollection services, string name, string authName, bool hasNamedM2MAuth)
        {
            services.AddHttpClient(name, (p, client) =>
            {
                var config = p.GetRequiredService<IConfiguration>();
                var logger = p.GetRequiredService<ILogger<T>>();

                client.BaseAddress = new Uri(config.Get($"{name}BaseAddress", logger));

                var namedM2MAuth = hasNamedM2MAuth ? name : string.Empty;

                string token = p.FetchMachineToMachineToken(authName, new ApiConfiguration
                {
                    ClientId = config.Get($"{namedM2MAuth}M2MAuthClientId", logger),
                    ClientSecret = config.Get($"{namedM2MAuth}M2MAuthClientSecret", logger),
                    Audience = config.Get($"{namedM2MAuth}M2MAuthAudience", logger, false),
                    UserName = config.Get($"{namedM2MAuth}M2MAuthUserName", logger, false),
                    Password = config.Get($"{namedM2MAuth}M2MAuthPassword", logger, false)

                }).Result;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            });

            return services;
        }

        public static IServiceCollection AddAuth0(this IServiceCollection services, string[]? namedAuth0Clients = null)
        {
            services.AddSingleton<IAuth0Cache>(p => new Auth0Cache(new Cache.MemoryCache(p.GetRequiredService<IMemoryCache>())));

            services.AddAuth0HttpClient();

            if (namedAuth0Clients == null)
            {
                return services;
            }

            foreach (var client in namedAuth0Clients)
            {
                services.AddAuth0HttpClient(true, client);
            }

            return services;
        }

        public static IServiceCollection AddAuth0HttpClient(this IServiceCollection services, bool hasNamedM2MAuth = false, string name = "")
        {
            var namedM2MAuth = hasNamedM2MAuth ? name : "";

            services.AddHttpClient($"Auth0{namedM2MAuth}", (p, client) =>
            {
                var config = p.GetRequiredService<IConfiguration>();
                var logger = p.GetRequiredService<ILogger<ServiceCollection>>();
                var domain = config.Get($"{namedM2MAuth}M2MAuthDomain", logger, true);

                client.BaseAddress = new Uri($"https://{domain}");
            });

            return services;
        }
        public static IServiceCollection AddCachedRestRepository<TID, TVALUE>(this IServiceCollection services, string apiName, Func<TID, string> getEndPoint, Func<TID, string> getListEndpoint = null, int cacheDurationInHours = 1)
        {
            services.AddSingleton<IReadRepository<TID, TVALUE>>((p) =>
            {
                var cache = p.GetRequiredService<IMemoryCache>();
                var repo = p.CreateRestRepository<TID, TVALUE>(apiName, getEndPoint, getListEndpoint);

                return new CachedRepository<TID, TVALUE>(repo, cache, TimeSpan.FromHours(cacheDurationInHours), nameof(TVALUE));
            });

            return services;
        }

        public static IReadRepository<TID, TVALUE> CreateRestRepository<TID, TVALUE>(this IServiceProvider p, string apiName, Func<TID, string> getEndPoint, Func<TID, string> getListEndpoint = null)
        {
            var api = p.CreateRestApi(apiName);

            return new RestRepositoryReader<TID, TVALUE>(api, getEndPoint, getListEndpoint);
        }

        public static IRestApi CreateRestApi(this IServiceProvider provider, string name)
        {
            return new RestApi(provider.GetRequiredService<IHttpClientFactory>(), name);
        }
        #region Private

        private static async Task<string> FetchMachineToMachineToken(this IServiceProvider services, string authName, ApiConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.Password))
            {
                if (string.IsNullOrWhiteSpace(config.Audience))
                    throw new ArgumentNullException("config.Audience");
            }
            else if (string.IsNullOrWhiteSpace(config.UserName))
                throw new ArgumentNullException("config.UserName");

            authName = $"Auth0{authName}";

            var cache = services.GetRequiredService<IAuth0Cache>();
            var logger = services.GetRequiredService<ILogger<ServiceCollection>>();
            var token = await cache.Cache.Get<string>("MachineToMachineTokens_" + authName);

            if (token != null)
                return token;

            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var tokenService = new Auth0TokenService(new RestApi(httpClientFactory, authName));

            try
            {
                var response = await tokenService.GetToken(config);

                try
                {
                    await cache.Cache.Add("MachineToMachineTokens_" + authName,
                                          response.AccessToken,
                                          response.ExpiresIn.HasValue ? TimeSpan.FromSeconds(response.ExpiresIn.Value - 100) : TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }

                return response.AccessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new HttpRequestException($"Failed to get access token", ex);
            }
        }

        #endregion
    }
}
