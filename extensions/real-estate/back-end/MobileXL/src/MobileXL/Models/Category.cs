using System;

namespace MobileXL.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ClientId { get; set; }
        public Guid SiteId { get; set; }
        public Guid ParentId { get; set; }
    }
}
