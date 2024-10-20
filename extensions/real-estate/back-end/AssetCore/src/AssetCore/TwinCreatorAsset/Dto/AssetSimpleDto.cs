using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Dto
{
    public class AssetSimpleDto
    {
        public Guid Id { get; set; }

        public Guid SiteId { get; set; }

        public Guid CategoryId { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public Guid? FloorId { get; set; }

        public string Name { get; set; }

        public string ForgeViewerModelId { get; set; }

        public string Identifier { get; set; }

        public Guid? CompanyId { get; set; }

        public List<double> Geometry { get; set; } = new List<double>();

        public Guid? EquipmentId { get; set; }

        public string ModuleTypeNamePath { get; set; }
    }
}
