using System.Text.Json.Serialization;

namespace WorkflowCore.Controllers.Request
{
    public class DynamicsUpdateTicketRequest
    {
        [JsonPropertyName("msft_wopriority")]
        public string Priority { get; set; }
        [JsonPropertyName("msft_wostatus")]
        public string TicketStatus { get; set; }
        [JsonPropertyName("msdyn_workordersummary")] 
        public string Summary { get; set; }
        [JsonPropertyName("msft_description")]
        public string Description { get; set; }     
        [JsonPropertyName("msft_requestorcontactnumber")]
        public string ReporterPhone { get; set; }
        [JsonPropertyName("msft_requestoremail")]
        public string ReporterEmail { get; set; }
        [JsonPropertyName("msft_requestorname")]
        public string ReporterName { get; set; }
    }
}
