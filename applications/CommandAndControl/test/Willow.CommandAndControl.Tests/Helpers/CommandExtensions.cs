using Willow.CommandAndControl.Data.Models;

namespace Willow.CommandAndControl.Tests.Helpers;

public static class CommandExtensions
{
    public static List<RequestedCommand> ToList(this RequestedCommand requestedCommand) => new() { requestedCommand };
    public static List<ResolvedCommand> ToList(this ResolvedCommand resolvedCommand) => new() { resolvedCommand };
    public static DateTimeOffset StartOfDay(this DateTimeOffset date)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, 0, 0, date.Offset);
    }
}
