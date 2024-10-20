using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Models
{
    public abstract class BaseAsset
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int BuildingId { get; set; }
        public int CategoryId { get; set; }
        public int? CompanyId { get; set; }
        public string FloorCode { get; set; }
        public string Name { get; set; }
        public string ApprovalStatus { get; set; }
        public bool CommentAlert { get; set; }
        public bool Archived { get; set; }
        public Guid? CurrentApprovalRoleId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? BatchId { get; set; }
        public DateTime? SyncDate { get; set; }
        public IEnumerable<int> FileIds { get; set; }
        public bool QrGenerated { get; set; }
        public bool QrScanned { get; set; }
        public string ExternalDeviceId { get; set; }
        public string ForgeViewerModelId { get; set; }
        public string Identifier { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}