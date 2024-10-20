namespace Connector.XL.Requests.Scan;

internal class ConnectorScanDto
{
    public Guid Id { get; set; }

    public Guid ConnectorId { get; set; }

    public ScanStatus Status { get; set; }

    public string Message { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string DevicesToScan { get; set; }

    public int? ErrorCount { get; set; }

    public string ErrorMessage { get; set; }

    public string Configuration { get; set; }

    internal static ConnectorScanDto MapFrom(ConnectorScan connectorScan)
    {
        return new ConnectorScanDto
        {
            Id = connectorScan.Id,
            Message = connectorScan.Message,
            Status = connectorScan.Status,
            ConnectorId = connectorScan.ConnectorId,
            CreatedAt = connectorScan.CreatedAt,
            CreatedBy = connectorScan.CreatedBy,
            EndTime = connectorScan.EndTime,
            ErrorCount = connectorScan.ErrorCount,
            ErrorMessage = connectorScan.ErrorMessage,
            StartTime = connectorScan.StartTime,
            DevicesToScan = connectorScan.DevicesToScan,
            Configuration = connectorScan.Configuration,
        };
    }

    internal static List<ConnectorScanDto> MapFrom(List<ConnectorScan> connectorScans)
    {
        return connectorScans.Select(MapFrom).ToList();
    }
}
