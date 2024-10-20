using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Willow.TelemetryGenerator.Options;

namespace Willow.TelemetryGenerator;

internal class SnowflakeErrorGenerator(IOptions<EventGridOptions> eventGridOptions) : BackgroundService
{
    private readonly SnowflakeTaskError errorMessage = new()
    {
        Version = "1.0",
        MessageId = "8c204974-b7d5-4916-8407-12df6db6ec0d",
        MessageType = "USER_TASK_FAILED",
        Timestamp = "2024-04-21T01:48:08.291Z",
        AccountName = "EE96414",
        TaskName = "PRD_DB.TRANSFORMED.CREATE_TABLE_UTILITY_BILLS_TK",
        TaskId = "01b2f373-92b4-6feb-0000-0000000005ef",
        RootTaskName = "PRD_DB.TRANSFORMED.CREATE_TABLE_UTILITY_BILLS_TK",
        RootTaskId = "01b2f373-92b4-6feb-0000-0000000005ef",
        Messages =
        [
            new()
            {
                RunId = "2024-04-21T01:08:00Z",
                ScheduledTime = "2024-04-21T01:08:00Z",
                QueryStartTime = "2024-04-21T01:08:06.969Z",
                CompletedTime = "2024-04-21T01:48:08.256Z",
                QueryId = "01b3ce84-0001-3398-0000-32790316a47a",
                ErrorCode = "000630",
                ErrorMessage = "Statement reached its statement or warehouse timeout of 2,400 second(s) and was canceled."
            }
        ]
    };

    private readonly SnowflakeSnowpipeError snowpipeErrorMessage = new()
    {
        Version = "1.0",
        MessageId = "a62e34bc-6141-4e95-92d8-f04fe43b43f5",
        MessageType = "INGEST_FAILED_FILE",
        Timestamp = "2021-10-22T19:15:29.471Z",
        AccountName = "MYACCOUNT",
        PipeName = "MYDB.MYSCHEMA.MYPIPE",
        TableName = "MYDB.MYSCHEMA.MYTABLE",
        StageLocation = "azure://myaccount.blob.core.windows.net/mycontainer/mypath",
        Messages =
        [
            new()
            {
                FileName = "/file1.csv_0_0_0.csv.gz",
                FirstError = "Numeric value 'abc' is not recognized"
            }
        ]
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventGridPublisherClient client;

        var cred = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
        {
            ExcludeManagedIdentityCredential = true,
            ExcludeWorkloadIdentityCredential = true,
            ExcludeEnvironmentCredential = true,
        });

        EventGridPublisherClientOptions clientOptions = new()
        {
            Diagnostics = { IsLoggingContentEnabled = true, IsLoggingEnabled = true, IsTelemetryEnabled = true },
        };

        if (eventGridOptions.Value.Key is null)
        {
            client = new EventGridPublisherClient(eventGridOptions.Value.TopicEndpoint, cred, clientOptions);
        }
        else
        {
            var key = new Azure.AzureKeyCredential(eventGridOptions.Value.Key);

            client = new(eventGridOptions.Value.TopicEndpoint, key, new());
        }

        PeriodicTimer timer = new(TimeSpan.FromSeconds(eventGridOptions.Value.Frequency));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            object message = eventGridOptions.Value.ErrorType?.ToLowerInvariant() switch
            {
                "task" => errorMessage,
                "snowpipe" => snowpipeErrorMessage,
                _ => errorMessage
            };

            var errorEvent = new EventGridEvent("https://willowinc.com/telgen/snowflake", "Snowflake.Errors", "1.0", message);

            try
            {
                await client.SendEventAsync(errorEvent, stoppingToken);

                Console.WriteLine("Event sent");
            }
            catch (Azure.RequestFailedException e)
            {
                Console.WriteLine($"Error sending event: {e}");

            }
        }
    }
}

internal record SnowflakeTaskErrorMessage
{
    public required string RunId { get; init; }
    public required string ScheduledTime { get; init; }
    public required string QueryStartTime { get; init; }
    public required string CompletedTime { get; init; }
    public required string QueryId { get; init; }
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
}

internal record SnowflakeTaskError
{
    public required string Version { get; init; }
    public required string MessageId { get; init; }
    public required string MessageType { get; init; }
    public required string Timestamp { get; init; }
    public required string AccountName { get; init; }
    public required string TaskName { get; init; }
    public required string TaskId { get; init; }
    public required string RootTaskName { get; init; }
    public required string RootTaskId { get; init; }
    public List<SnowflakeTaskErrorMessage> Messages { get; init; } = [];
}


internal record SnowflakeSnowpipeErrorMessage
{
    public string FileName { get; init; }
    public string FirstError { get; init; }
}

internal record SnowflakeSnowpipeError
{
    public string Version { get; init; }
    public string MessageId { get; init; }
    public string MessageType { get; init; }
    public string Timestamp { get; init; }
    public string AccountName { get; init; }
    public string PipeName { get; init; }
    public string TableName { get; init; }
    public string StageLocation { get; init; }
    public List<SnowflakeSnowpipeErrorMessage> Messages { get; init; }
}
