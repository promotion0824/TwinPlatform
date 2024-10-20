using System.Collections.Generic;

namespace AssetCoreTwinCreator.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int? ApprovalId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int? ExpectedRecordCount { get; set; }
        public string RevitTypeCode { get; set; }
        public string RevitModelPrefix { get; set; }
        public bool HaveAccess { get; set; }
        public bool HasTemplateAssigned { get; set; }

        public IEnumerable<int> GroupIds { get; set; }

        public List<Category> ChildCategories { get; set; } = new List<Category>();

        public int AssetCount { get; set; }

        public bool HasChildren { get; set; }
    }
}
