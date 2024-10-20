using System.Threading.Tasks;
using Azure.Core;
using DigitalTwinCore.Models;

namespace DigitalTwinCore.Services.AdtApi;

public interface ITokenService
{
    Task<AccessToken> GetAccessToken(AzureDigitalTwinsSettings instanceSettings);
}