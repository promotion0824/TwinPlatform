using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketIssueDto
    {
        public Guid Id { get; set; }
        public TicketIssueType Type { get; set; }
        public string Name { get; set; }
        public string TwinId { get; set; }

        public static TicketIssueDto MapFromModel(Equipment equipment)
        {
            return new TicketIssueDto
            {
                Id = equipment.Id,
                Type = TicketIssueType.Equipment,
                Name = equipment.Name,
            };
        }

        public static TicketIssueDto MapFromModel(Asset asset)
        {
            return new TicketIssueDto
            {
                Id = asset.Id,
                Type = TicketIssueType.Asset,
                Name = asset.Name,
                TwinId = asset.TwinId
            };
        }


        public static List<TicketIssueDto> MapFromModels(List<Equipment> equipments)
        {
            return equipments.Select(MapFromModel).ToList();
        }

        public static List<TicketIssueDto> MapFromModels(List<Asset> assets)
        {
            return assets.Select(MapFromModel).ToList();
        }
    }
}
