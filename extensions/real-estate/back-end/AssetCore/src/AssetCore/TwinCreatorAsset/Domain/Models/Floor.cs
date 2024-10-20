using System.Collections.Generic;

namespace AssetCoreTwinCreator.Domain.Models
{
    public class Floor
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public string Code { get; set; }

        public Building Building { get; set; }
    }
}
