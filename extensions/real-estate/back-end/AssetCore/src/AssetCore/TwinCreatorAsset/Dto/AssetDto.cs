using System;
using System.Collections.Generic;
using System.Linq;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Dto
{
    public class AssetDtoBase : AssetSimpleDto
    {
        public int ClientId { get; set; }
        
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
    }

    public class AssetDto : AssetDtoBase
    {
        public int ValidationError { get; set; }
        
        // TODO: The Maintenance Responsibility should be a nullable field on the TES_Asset_Register. Otherwise we are matching by string.
        public string MaintenanceResponsibility => AssetParameters != null ? AssetParameters.FirstOrDefault(x => x.Key == "MaintenanceResponsibility")?.Value?.ToString() : "";
        public IEnumerable<AssetParameter> AssetParameters { get; set; }
        
    }

}
