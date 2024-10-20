using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace Scheduler.Services
{
    public interface IWorkflowApi
    {
        Task GenerateInspectionRecords();
        Task SendInspectionDailyReport();
        Task CheckSchedules();
    }

    public class WorkflowApi : IWorkflowApi
    {
        private readonly ILogger<WorkflowApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkflowApi(IHttpClientFactory httpClientFactory, ILogger<WorkflowApi> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task GenerateInspectionRecords()
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.WorkflowCore);
            var response = await client.PostAsJsonAsync("inspectionRecords/generate", new object());
            _logger.LogInformation($"Status Code: {response.StatusCode}");
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response Body: {responseBody}");
            response.EnsureSuccessStatusCode();
        }

        public async Task SendInspectionDailyReport()
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.WorkflowCore);
            var response = await client.PostAsJsonAsync("inspections/reports", new object());
            _logger.LogInformation($"Status Code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
        }

        public async Task CheckSchedules()
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.WorkflowCore);
            var response = await client.PostAsJsonAsync("schedules/check", new object());
            _logger.LogInformation($"Status Code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
        }
    }
}
