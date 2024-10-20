using System;
using System.Linq;
using System.Collections.Generic;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketAssetDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; }

        public static TicketAssetDto MapFromModel(TicketAsset ticketAsset)
        {
            return new TicketAssetDto
            {
                Id = ticketAsset.Id,
                AssetId = ticketAsset.AssetId,
                AssetName = ticketAsset.AssetName
            };
        }

        public static List<TicketAssetDto> MapFromModels(List<TicketAsset> ticketAssets)
        {
            return ticketAssets?.Select(MapFromModel).ToList();
        }
    }
}
