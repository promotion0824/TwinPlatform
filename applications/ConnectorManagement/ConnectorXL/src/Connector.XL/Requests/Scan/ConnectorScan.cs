namespace Connector.XL.Requests.Scan;

internal class ConnectorScan
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
}
