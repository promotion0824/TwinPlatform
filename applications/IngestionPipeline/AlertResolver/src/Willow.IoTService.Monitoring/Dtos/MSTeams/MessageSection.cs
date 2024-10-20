using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Dtos.MSTeams
{
    public class MessageSection
    {
        public string? ActivityTitle { get; set; }

        public IEnumerable<MessageFact> Facts { get; set; } = new List<MessageFact>();
    }
}