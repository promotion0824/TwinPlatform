using System;
using System.Linq;
using System.Collections.Generic;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketTwinDto
    {
        public Guid TwinId { get; set; }
        public string TwinName { get; set; }

        public static TicketTwinDto MapFromModel(TicketTwin ticketTwin)
        {
            if (ticketTwin == null)
            {
                return null; 
            }

            return new TicketTwinDto
            {
                TwinId = ticketTwin.TwinId,
                TwinName = ticketTwin.TwinName
            };
        }

        public static List<TicketTwinDto> MapFromModels(List<TicketTwin> ticketTwins)
        {
            return ticketTwins?.Select(MapFromModel).ToList();
        }
    }
}
