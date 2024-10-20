using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.ServicesApi.ZendeskApi;
public class ZendeskCreateTicketRequest
{
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
    [JsonPropertyName("comment")]
    public ZendeskTicketComment Comment { get; set; }
    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }
    [JsonPropertyName("status")]
    public ZendeskTicketStatus Status { get; set; }
    [JsonPropertyName("priority")]
    public ZendeskTicketPriority? Priority { get; set; }
    [JsonPropertyName("requester")]
    public ZendeskTicketRequester Requester { get; set; }
    [JsonPropertyName("custom_fields")]
    public List<ZendeskCustomField> CustomFields { get; set; }
}

[Description("custom_field")]
public class ZendeskCustomField
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    public string Value { get; set; }
    public List<string> Values { get; set; }
}

public class ZendeskTicketComment
{

    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("uploads")]
    public List<string> Uploads { get; set; }

}

[Description("Requester")]
public class ZendeskTicketRequester
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }

}
