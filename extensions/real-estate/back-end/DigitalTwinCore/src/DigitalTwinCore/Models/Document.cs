using System;

namespace DigitalTwinCore.Models
{
    public class Document
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }
    }
}
