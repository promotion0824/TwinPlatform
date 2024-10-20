namespace Willow.IoTService.Monitoring.Options;

public class PagerDutyEmailOptions
{
    /// <summary>
    /// Comma delimited list of recipient email addresses
    /// </summary>
    public string? RecipientEmailAddresses { get; set; }
    public string? FromEmailAddress { get; set; }
    public string? FromName { get; set; }
    public string? SendGridApiToken { get; set; }
}
