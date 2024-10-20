using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Dto
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public int? ApprovalId { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public int? ExpectedRecordCount { get; set; }
        public string RevitTypeCode { get; set; }
        public string RevitModelPrefix { get; set; }
        public bool HaveAccess { get; set; }
        public bool HasTemplateAssigned { get; set; }

        public IEnumerable<int> GroupIds { get; set; }

        public List<CategoryDto> ChildCategories { get; set; }

        public int AssetCount { get; set; }

        public bool HasChildren { get; set; }
        public string ModuleTypeNamePath { get; set; }
    }
}
