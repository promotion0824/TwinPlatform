using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AdminPortalXL.Models.Directory;

namespace AdminPortalXL.Services
{
    public interface IDirectoryApiService
    {
        Task<AuthenticationInfo> GetAuthenticationToken(string authorizationCode, string redirectUri);
        Task<Supervisor> GetSupervisor(Guid supervisorId);
        Task SendResetPasswordEmail(string supervisorEmail);
        Task<ResetPasswordToken> GetResetPasswordToken(string token);
        Task UpdatePassword(string supervisorEmail, string password, string resetPasswordToken);
    }

    public class DirectoryApiService : IDirectoryApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DirectoryApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AuthenticationInfo> GetAuthenticationToken(string authorizationCode, string redirectUri)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var url = $"signIn?authorizationCode={authorizationCode}&redirectUri={redirectUri}";
                var response = await client.PostAsync(url, null);
                response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<AuthenticationInfo>();
            }
        }

        public async Task<ResetPasswordToken> GetResetPasswordToken(string token)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"supervisors/resetPasswordTokens/{token}");
                response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<ResetPasswordToken>();
            }
        }

        public async Task<Supervisor> GetSupervisor(Guid supervisorId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.GetAsync($"supervisors/{supervisorId}");
                response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
                return await response.Content.ReadAsAsync<Supervisor>();
            }
        }

        public async Task SendResetPasswordEmail(string supervisorEmail)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PostAsJsonAsync($"supervisors/{supervisorEmail}/password/reset", new { });
                response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }

        public async Task UpdatePassword(string supervisorEmail, string password, string resetPasswordToken)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DirectoryCore))
            {
                var response = await client.PutAsJsonAsync($"supervisors/{supervisorEmail}/password", new { Password = password, EmailToken = resetPasswordToken });
                response.EnsureSuccessStatusCode(ApiServiceNames.DirectoryCore);
            }
        }
    }
}
