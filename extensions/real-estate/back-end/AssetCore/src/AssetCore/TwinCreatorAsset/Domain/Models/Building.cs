using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Domain.Models
{
    public class Building
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Enabled { get; set; }
        public bool ShowDashboard { get; set; }
        public bool AssetExplorer { get; set; }
        public bool AssetRegister { get; set; }
        public Guid? SpaceId { get; set; }
        public byte[] Image { get; set; }
        public DateTime? TimelineStartDate { get; set; }
        public DateTime? TimelineEndDate { get; set; }
        public string TimeZone { get; set; }

        public IEnumerable<Floor> Floors { get; set; }
    }
}
