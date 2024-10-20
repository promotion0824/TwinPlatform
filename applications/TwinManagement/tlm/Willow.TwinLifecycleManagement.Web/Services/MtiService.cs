using System.Text;
using System.Text.Json;
using Willow.TwinLifecycleManagement.Web.Diagnostic;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public interface IMtiService
    {
        public Task<HttpResponseMessage> SyncOrganization(string userId, params object[] parameters);

        public Task<HttpResponseMessage> SyncSpatial(string userId, params object[] parameters);

        public Task<HttpResponseMessage> SyncAssets(string userId, params object[] parameters);

        public Task<HttpResponseMessage> SyncCapabilities(string userId, params object[] parameters);
    }

    public class MtiService : IMtiService
    {
        private readonly HttpClient _mtiClient;
        private readonly HealthCheckMTI _healthCheckMti;

        public MtiService(IHttpClientFactory httpClientFactory, HealthCheckMTI healthCheckMTI)
        {
            _mtiClient = httpClientFactory.CreateClient("MTIAPI");
            _healthCheckMti = healthCheckMTI;
        }

        public async Task<HttpResponseMessage> SyncOrganization(string userId, params object[] parameters)
        {
            return await MakePostRequest("/Sync/SyncOrganization", "2" ,userId, autoApprove: (bool)parameters[0]);
        }

        public async Task<HttpResponseMessage> SyncSpatial(string userId, object[] parameters)
        {
            return await MakePostRequest("/Sync/SyncSpatial", "2", userId, (string[])parameters[0], autoApprove: (bool)parameters[1]);
        }

        public async Task<HttpResponseMessage> SyncAssets(string userId, object[] parameters)
        {
            return await MakePostRequest("/Sync/SyncAssets", "2", userId, (string[])parameters[0], (string)parameters[1], autoApprove: (bool)parameters[2]);
        }

        public async Task<HttpResponseMessage> SyncCapabilities(string userId, object[] parameters)
        {
            return await MakePostRequest("/Sync/SyncCapabilities", "2", userId, (string[])parameters[0], (string)parameters[1], autoApprove: (bool)parameters[2], matchStdPntList: (bool)parameters[3] );
        }

        private async Task<HttpResponseMessage> MakePostRequest(string endpoint, string version, string userId = null, string[] buildingIds = null, string connectorId = null, bool? autoApprove = null, bool? matchStdPntList = null)
        {
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append(endpoint);

            if (connectorId != null || userId != null || autoApprove != null || matchStdPntList != null)
            {
                urlBuilder.Append("?");
            }

            if (userId != null)
            {
                urlBuilder.Append($"userId={Uri.EscapeDataString(userId)}");

                if (connectorId != null || autoApprove != null || matchStdPntList != null)
                {
                    urlBuilder.Append("&");
                }
            }

            if (connectorId != null)
            {
                urlBuilder.Append($"connectorId={Uri.EscapeDataString(connectorId)}");

                if (autoApprove != null || matchStdPntList != null)
                {
                    urlBuilder.Append("&");
                }
            }

            if (autoApprove != null)
            {
                urlBuilder.Append($"autoApprove={autoApprove}");

                if (matchStdPntList != null)
                {
                    urlBuilder.Append("&");
                }
            }

            if (matchStdPntList != null)
            {
                urlBuilder.Append($"matchStdPntList={matchStdPntList}");
            }

            try
            {
                _mtiClient.DefaultRequestHeaders.Add("api-version", version);

                string contentJson = buildingIds != null ? JsonSerializer.Serialize(buildingIds) : "[]"; // Send an empty object if buildingIds is null

                var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _mtiClient.PostAsync(urlBuilder.ToString(), content);
                _healthCheckMti.Current = HealthCheckMTI.Healthy;
                return response;
            }
            catch (Exception ex)
            {
                var healthCheck = HealthCheckMTI.FailingCalls;
                _healthCheckMti.Current = healthCheck;
                throw;
            }
        }
    }
}
