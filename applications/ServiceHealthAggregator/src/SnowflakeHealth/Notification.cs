namespace Willow.ServiceHealthAggregator.Snowflake;

using System.Dynamic;
using System.Text.Json;

internal class Notification
{
    private string? data;
    private IDictionary<string, object>? dataDictionary;

    public required string Id { get; init; }

    public required string Subject { get; init; }

    public string? Data
    {
        get => data;
        init
        {
            data = value;
            if (data != null)
            {
                try
                {
                    ExpandoObject? dataObj = JsonSerializer.Deserialize<ExpandoObject>(data);

                    dataDictionary = dataObj!;
                }
                catch
                {
                }
            }
        }
    }

    public required DateTimeOffset EnqueuedTime { get; init; }

    public string AccountName => GetProperty("accountName") ?? string.Empty;

    public string TaskOrPipeName => GetProperty("taskName") ?? GetProperty("pipeName") ?? string.Empty;

    private string? GetProperty(string propertyName)
    {
        if (dataDictionary == null || !dataDictionary.TryGetValue(propertyName, out object? value) || value == null)
        {
            return null;
        }

        return value.ToString()!;
    }
}
