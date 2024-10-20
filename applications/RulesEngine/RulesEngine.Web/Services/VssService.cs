// using System;
// using System.Collections.Generic;
// using System.Net.Http.Headers;
// using System.Threading;
// using System.Threading.Tasks;
// using Azure.Core;
// using Azure.Identity;
// using Microsoft.Extensions.Logging;
// using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
// using Microsoft.VisualStudio.Services.Client;
// using Microsoft.VisualStudio.Services.WebApi;

// namespace RulesEngine.Web.Services;

// /// <summary>
// /// A service for creating bugs and other Azure DevOps work items
// /// </summary>
// public interface IVssService
// {

// }


// public class VssService : IVssService
// {
//     private const string AdoBaseUrl = "https://dev.azure.com";
//     private const string AdoOrgName = "willowdev";

//     /// <summary>
//     /// Azure AD tenant id
//     /// </summary>
//     public const string AadTenantId = "d43166d1-c2a1-4f26-a213-f620dba13ab8";

//     /// <summary>
//     /// ClientId for User Assigned Managed Identity. Leave null for System Assigned Managed Identity
//     /// </summary>
//     public const string AadUserAssignedManagedIdentityClientId = null;

//     // Credentials object is static so it can be reused across multiple requests. This ensures
//     // the internal token cache is used which reduces the number of outgoing calls to Azure AD to get tokens.
//     //
//     // DefaultAzureCredential will use VisualStudioCredentials or other appropriate credentials for local development
//     // but will use ManagedIdentityCredential when deployed to an Azure Host with Managed Identity enabled.
//     // https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet#defaultazurecredential
//     private readonly static TokenCredential credential =
//         new DefaultAzureCredential(
//             new DefaultAzureCredentialOptions
//             {
//                 TenantId = AadTenantId,
//                 ExcludeEnvironmentCredential = true
//             });
//     private readonly ILogger<VssService> logger;

//     public static List<ProductInfoHeaderValue> AppUserAgent { get; } = new()
//     {
//         new ProductInfoHeaderValue("Identity.ManagedIdentitySamples", "1.0"),
//         new ProductInfoHeaderValue("(3-AzureFunction-ManagedIdentity)")
//     };

//     /// <summary>
//     /// Initializes a new instance of the <see cref="VssService"/> class.
//     /// </summary>
//     public VssService(ILogger<VssService> logger)
//     {
//         this.logger = logger;
//     }

//     public async Task<IActionResult> Run()
//     {
//         var vssConnection = await CreateVssConnection();
//         var workItemTrackingHttpClient = vssConnection.GetClient<WorkItemTrackingHttpClient>();

//         try
//         {
//             var workItem = await workItemTrackingHttpClient.GetWorkItemAsync(workItemId);

//             workItem.Fields.TryGetValue("System.Title", out var title);
//             var responseMessage = $"Work item '{title}' fetched. This HTTP triggered function executed successfully.";
//             return new OkObjectResult(responseMessage);
//         }
//         catch (Exception ex)
//         {
//             return new ObjectResult(ex.Message);
//         }
//     }

//     private static async Task<VssConnection> CreateVssConnection()
//     {
//         var accessToken = await GetManagedIdentityAccessToken();
//         var token = new VssAadToken("Bearer", accessToken);
//         var credentials = new VssAadCredential(token);

//         var settings = VssClientHttpRequestSettings.Default.Clone();
//         settings.UserAgent = AppUserAgent;

//         var organizationUrl = new Uri(new Uri(AdoBaseUrl), AdoOrgName);
//         return new VssConnection(organizationUrl, credentials, settings);
//     }

//     private static async Task<string> GetManagedIdentityAccessToken()
//     {
//         var tokenRequestContext = new TokenRequestContext(VssAadSettings.DefaultScopes);
//         var token = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

//         return token.Token;
//     }

// }
