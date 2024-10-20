using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class AssetCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ModuleTypeNamePath { get; set; }
        public List<AssetCategory> Categories { get; set; }
        public List<Asset> Assets { get; set; }
    }
}
