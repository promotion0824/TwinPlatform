using System;
using System.Collections.Generic;

namespace AssetCoreTwinCreator.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int? ApprovalId { get; set; }
        public int? ParentId { get; set; }      
        public string Name { get; set; }
        public Guid? SpaceId { get; set; }
        public int? ExpectedRecordCount { get; set; }
        public int? DbObjectId { get; set; }
        public string DbTableName { get; set; }
        public string RevitTypeCode { get; set; }
        public string RevitModelPrefix { get; set; }
        public bool Archived { get; set; }

        public virtual Building Building { get; set; }
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<CategoryGroup> CategoryGroups { get; set; }
        public virtual ICollection<CategoryColumn> CategoryColumns { get; set; }
    }
}