using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.ServicesApi.ZendeskApi;
public class ZendeskTicket
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// The API url of this ticket
    /// </summary>
    [JsonPropertyName("url")]
    public Uri Url { get; set; }
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }

}

public enum ZendeskTicketSeverityLevel
{
    [Description( "1_-_Critical")]
    Critical,
    [Description("2_-_Medium")]
    Medium,
    [Description("3_-_Low")]
    Low,
    [Description("4_-_Informational")]
    Informational
}
public enum ZendeskTicketStatus
{
    New,
    Open,
    Closed,
    Pending,
    Solved,
    Hold,
    Deleted
}
public enum ZendeskTicketPriority
{
    Urgent,
    High,
    Normal,
    Low
}
