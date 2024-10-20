using System;

namespace SiteCore.Domain
{
    public class Module
    {
        public Guid Id { get;set; }
        public string Name { get; set; }
        public Guid FloorId { get; set; }
        public Guid ModuleTypeId { get; set; }
        public Guid VisualId { get; set; }
        public ModuleType ModuleType { get; set; }
        public string Path { get; set; }
    }
}
