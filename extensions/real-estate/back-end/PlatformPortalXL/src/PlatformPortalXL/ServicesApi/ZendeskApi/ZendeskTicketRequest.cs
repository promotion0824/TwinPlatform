using System.Text.Json.Serialization;

namespace PlatformPortalXL.ServicesApi.ZendeskApi
{
    public class ZendeskTicketRequest<T>
    {
        public ZendeskTicketRequest(T ticketCreateRequest)
        {
            Ticket = ticketCreateRequest;
        }

        [JsonPropertyName("ticket")]
        public T Ticket { get; set; }
    }
}
