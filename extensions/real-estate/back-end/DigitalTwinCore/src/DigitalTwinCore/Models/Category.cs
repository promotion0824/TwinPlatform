using System;
using System.Collections.Generic;

namespace DigitalTwinCore.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
        public List<Asset> Assets { get; set; }
    }
}
