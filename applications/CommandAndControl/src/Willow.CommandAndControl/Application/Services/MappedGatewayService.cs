namespace Willow.CommandAndControl.Application.Services;

using Polly;

internal class MappedGatewayService : IMappedGatewayService
{
    private const int DefaultTimeoutSecs = 30;
    private readonly HttpClient httpClient;

    public MappedGatewayService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<SetValueResponse> SendSetValueCommandAsync(string pointId, double value)
    {
        ArgumentException.ThrowIfNullOrEmpty(pointId);
        var policy = Policy
            .Handle<Exception>()
            .OrResult<SetValueResponse>(x => x.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync($"points/{pointId}/commands/SetValue", new SetValueRequest(value.ToString(), DefaultTimeoutSecs));
            var result = await response.Content.ReadFromJsonAsync<SetValueResponse>();
            if (result is not null)
            {
                result.StatusCode = response.StatusCode;
            }

            return result ?? new SetValueResponse { StatusCode = HttpStatusCode.InternalServerError };
        });

        return result ?? new SetValueResponse { StatusCode = HttpStatusCode.InternalServerError };
    }
}
