namespace Willow.PublicApi.Authorization;

using global::Authorization.TwinPlatform.Common.Model;

internal class Permissions
{
    private static Dictionary<string, ImportPermission>? importPermissions;

    internal static Dictionary<string, ImportPermission> ImportPermissions
    {
        get
        {
            return importPermissions ??= new()
            {
                [TimeSeries.Read] = new() { Name = TimeSeries.Read, Description = "Read Time Series" },
                [TimeSeries.Write] = new() { Name = TimeSeries.Write, Description = "Write Time Series" },
                [Twins.Read] = new() { Name = Twins.Read, Description = "Read Twins" },
                [Twins.Write] = new() { Name = Twins.Write, Description = "Write Twins" },
                [Models.Read] = new() { Name = Models.Read, Description = "Read Models" },
                [Models.Write] = new() { Name = Models.Write, Description = "Write Models" },
                [Insights.Read] = new() { Name = Insights.Read, Description = "Read Insights" },
                [Insights.Write] = new() { Name = Insights.Write, Description = "Write Insights" },
                [Tickets.Read] = new() { Name = Tickets.Read, Description = "Read Tickets" },
                [Tickets.Write] = new() { Name = Tickets.Write, Description = "Write Tickets" },
                [Inspections.Read] = new() { Name = Inspections.Read, Description = "Read Inspections" },
                [Inspections.Write] = new() { Name = Inspections.Write, Description = "Write Inspections" },
                [Documents.Read] = new() { Name = Documents.Read, Description = "Read Documents" },
                [Documents.Write] = new() { Name = Documents.Write, Description = "Write Documents" },
            };
        }
    }

    internal class TimeSeries
    {
        public const string Read = $"{nameof(TimeSeries)}.{nameof(Read)}";
        public const string Write = $"{nameof(TimeSeries)}.{nameof(Write)}";
    }

    internal class Twins
    {
        public const string Read = $"{nameof(Twins)}.{nameof(Read)}";
        public const string Write = $"{nameof(Twins)}.{nameof(Write)}";
    }

    internal class Models
    {
        public const string Read = $"{nameof(Models)}.{nameof(Read)}";
        public const string Write = $"{nameof(Models)}.{nameof(Write)}";
    }

    internal class Insights
    {
        public const string Read = $"{nameof(Insights)}.{nameof(Read)}";
        public const string Write = $"{nameof(Insights)}.{nameof(Write)}";
    }

    internal class Tickets
    {
        public const string Read = $"{nameof(Tickets)}.{nameof(Read)}";
        public const string Write = $"{nameof(Tickets)}.{nameof(Write)}";
    }

    internal class Inspections
    {
        public const string Read = $"{nameof(Inspections)}.{nameof(Read)}";
        public const string Write = $"{nameof(Inspections)}.{nameof(Write)}";
    }

    internal class Documents
    {
        public const string Read = $"{nameof(Documents)}.{nameof(Read)}";
        public const string Write = $"{nameof(Documents)}.{nameof(Write)}";
    }
}
