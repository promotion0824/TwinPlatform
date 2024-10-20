namespace Willow.ServiceHealthAggregator.Snowflake;

using System.Dynamic;
using System.Text;
using System.Text.Json;

internal static class NotificationExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static string ToTeamsMessageString(this Notification notification, string instance)
    {
        string dataJson = FormatJsonString(notification.Data);

        return $@"
        {{
            ""@type"": ""MessageCard"",
            ""@context"": ""https://schema.org/extensions"",
            ""summary"": ""Error Notification"",
            ""themeColor"": ""d70000"",
            ""title"": ""Error Notification"",
            ""sections"": [
                {{
                    ""facts"": {FormatNotificationDetails(notification, instance)}
                }},
                {{
                    ""type"": ""TextBlock"",
                    ""text"": ""{dataJson}"",
                    ""wrap"": true
                }}
            ]
        }}";
    }

    public static string ToEmailBodyString(this Notification notification, string instance)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Notification Details:<br/>");
        sb.AppendLine("Source: Snowflake<br/>");
        sb.AppendLine($"Enqueued Time: {notification.EnqueuedTime.ToString("O")}<br/>");
        sb.AppendLine($"Account Name: {notification.AccountName}<br/>");
        sb.AppendLine($"Task/Pipe Name: {notification.TaskOrPipeName}<br/>");
        sb.AppendLine($"Instance: {instance}<br/>");
        sb.AppendLine("<br/>");
        sb.AppendLine("Details:<br/>");

        sb.Append("<pre>");
        sb.Append(notification.Data);
        sb.AppendLine("</pre>");

        return sb.ToString();
    }

    private static string FormatNotificationDetails(Notification notification, string instance)
    {
        var details = new List<Dictionary<string, string>>
        {
            new() { { "name", "Source" }, { "value", "Snowflake" } },
            new() { { "name", "Enqueued Time" }, { "value", notification.EnqueuedTime.ToString("O") } },
            new() { { "name", "Account Name" }, { "value", notification.AccountName } },
            new() { { "name", "Task/Pipe Name" }, { "value", notification.TaskOrPipeName } },
            new() { { "name", "Instance" }, { "value", instance } },
        };

        return JsonSerializer.Serialize(details, JsonSerializerOptions);
    }

    private static string FormatJsonString(string? data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        return data.Replace("\"", "\\\"");
    }
}
