using System;

namespace AssetCore.TwinCreatorAsset.Dto
{
    public class AssetTreeAssetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? EquipmentId { get; set; }
        public Guid? FloorId { get; set; }
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
    }
}
