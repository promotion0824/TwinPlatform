using System.Collections.Generic;

namespace AssetCoreTwinCreator.Domain.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClientId { get; set; }

        public virtual ICollection<CategoryGroup> CategoryGroups { get; set; }
    }
}