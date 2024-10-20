using System;

namespace MobileXL.Models
{
    public class TicketAsset
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid AssetCategoryId { get; set; }
        public string AssetName { get; set; }
    }

}
