namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.DownloadActivityLogs;

using System;
using System.Globalization;
using CsvHelper;
using Willow.CommandAndControl.Application.Extensions;

internal static class DownloadActivityLogsHandler
{
    internal static async Task<Results<FileStreamHttpResult, BadRequest<ProblemDetails>>> HandleAsync([FromBody] DownloadActivityLogsRequestDto request,
        IApplicationDbContext dbContext,
        [FromRoute] string? format = "csv",
        CancellationToken cancellationToken = default)
    {
        (int total, List<ActivityLog> list) = await dbContext.ActivityLogs.Include(a => a.RequestedCommand).FilterBy(request.FilterSpecifications).SortBy(request.SortSpecifications).GetResultAsync(null,
                                                    cancellationToken);

        var result = list.Select(x => new DownloadActivityLogsResponseDto
        {
            Location = x.RequestedCommand.Location,
            TwinId = x.RequestedCommand.TwinId,
            Parent = x.RequestedCommand.IsCapabilityOf ?? x.RequestedCommand.IsHostedBy,
            CommandName = x.RequestedCommand.CommandName,
            UpdatedBy = x.UpdatedBy,
            Type = x.Type,
            Timestamp = x.Timestamp,
            Description = x.ToDescription(),
        });

        return format switch
        {
            "csv" => await result.ToCsvAsync(),
            "pdf" => throw new NotSupportedException("PDFs are not supported yet"), // Just a placeholder for now
            _ => throw new ArgumentException($"Format {format} is invalid for type {nameof(DownloadActivityLogsRequestDto)}"),
        };
    }

    private static async Task<FileStreamHttpResult> ToCsvAsync(this IEnumerable<DownloadActivityLogsResponseDto> result)
    {
        MemoryStream stream = new();
        var writer = new StreamWriter(stream);
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<DownloadActivityLogsResponseDto>();
        csv.NextRecord();
        await csv.WriteRecordsAsync(result);
        csv.Flush();

        stream.Position = 0;

        return TypedResults.File(stream, "text/csv", "ActivityLogs.csv");
    }

    private static string ToDescription(this ActivityLog activityLog)
    {
        switch (activityLog.Type)
        {
            case ActivityType.Received:
                string endTime = activityLog.RequestedCommand.EndTime != null ? $" until {activityLog.RequestedCommand.EndTime:HH:mm}" : string.Empty;
                return $@"Command received to set value {activityLog.RequestedCommand.Value} {activityLog.RequestedCommand.Unit} from
                        {activityLog.RequestedCommand.StartTime:HH:mm} {endTime}";
            case ActivityType.Approved:
            case ActivityType.Cancelled:
            case ActivityType.Executed:
            case ActivityType.Failed:
            case ActivityType.Retracted:
            case ActivityType.Completed:
            case ActivityType.Suspended:
            case ActivityType.Retried:
                return $"Command was {activityLog.Type.ToString().ToLowerInvariant()}";
            case ActivityType.MessageSent:
                return $"Write request {activityLog.RequestedCommand.Value} {activityLog.RequestedCommand.Unit} sent for {activityLog.RequestedCommand.StartTime:MMM d, yyyy, HH:mm}";
            case ActivityType.MessageReceivedFailed:
                return "Write request status Failed";
            case ActivityType.MessageReceivedSuccess:
                return "Write request status Success";
            default:
                return string.Empty;
        }
    }
}
