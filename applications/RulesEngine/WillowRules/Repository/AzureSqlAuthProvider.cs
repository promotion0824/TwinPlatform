using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace Willow.Rules.Repository;

/// <summary>
/// Sql Auth provider using Default Azure Credential
/// </summary>
public class AzureSqlAuthProvider : SqlAuthenticationProvider
{
	private static readonly string[] _azureSqlScopes = new[]
	{
			"https://database.windows.net//.default"
		};
	private readonly DefaultAzureCredential defaultAzureCredential;

	/// <summary>
	/// Creates a new instance of the see <cref = "AzureSqlAuthProvider" />
	/// </summary>
	public AzureSqlAuthProvider(DefaultAzureCredential defaultAzureCredential)
	{
		this.defaultAzureCredential = defaultAzureCredential ?? throw new System.ArgumentNullException(nameof(defaultAzureCredential));
	}

	public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
	{
		var tokenRequestContext = new TokenRequestContext(_azureSqlScopes);
		var tokenResult = await defaultAzureCredential.GetTokenAsync(tokenRequestContext, default);
		return new SqlAuthenticationToken(tokenResult.Token, tokenResult.ExpiresOn);
	}

	public override bool IsSupported(SqlAuthenticationMethod authenticationMethod) =>
		authenticationMethod.Equals(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity);
}
