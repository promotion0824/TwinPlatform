namespace Willow.LiveData.Core.Features.Connectors.Models;

using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class ConnectorState
{
    public Guid ConnectorId { get; set; }

    public string ConnectionType { get; set; }

    public DateTime TimestampUtc { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsArchived { get; set; }

    public int? Interval { get; set; }
}
