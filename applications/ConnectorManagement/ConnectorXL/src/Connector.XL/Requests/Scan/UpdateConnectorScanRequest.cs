namespace Connector.XL.Requests.Scan;

internal class UpdateConnectorScanRequest
{
    public ScanStatus Status { get; set; }

    public string ErrorMessage { get; set; }

    public int? ErrorCount { get; set; }

    public DateTime? Started { get; set; }

    public DateTime? Finished { get; set; }
}
