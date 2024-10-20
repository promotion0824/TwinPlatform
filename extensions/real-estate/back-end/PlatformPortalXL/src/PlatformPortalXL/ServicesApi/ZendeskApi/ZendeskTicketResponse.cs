using System.Text.Json.Serialization;

namespace PlatformPortalXL.ServicesApi.ZendeskApi;
public class ZendeskTicketResponse
{
    [JsonPropertyName("ticket")]
    public ZendeskTicket Ticket { get; set; }
}

