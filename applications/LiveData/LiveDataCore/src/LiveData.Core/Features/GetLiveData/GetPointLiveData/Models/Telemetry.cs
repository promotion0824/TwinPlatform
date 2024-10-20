namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;

using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class Telemetry
{
    public int RowNumber { get; set; }

    public string ConnectorId { get; set; }

    public string DtId { get; set; }

    public string ExternalId { get; set; }

    public string TrendId { get; set; }

    public DateTime SourceTimestamp { get; set; }

    public DateTime EnqueuedTimestamp { get; set; }

    public dynamic ScalarValue { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal Altitude { get; set; }

    public dynamic Properties { get; set; }
}
