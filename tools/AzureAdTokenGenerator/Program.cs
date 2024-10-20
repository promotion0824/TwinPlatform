
using Azure.Core;
using Azure.Identity;

var azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
var audience = "api://742a5de4-db47-418b-b8a8-acdd5ab6ea39/.default";
var accessToken = azureCredential.GetToken(new TokenRequestContext(new []{audience}));

Console.WriteLine($"AzureAd Token was successfuly generated:\n\n {accessToken.Token}");