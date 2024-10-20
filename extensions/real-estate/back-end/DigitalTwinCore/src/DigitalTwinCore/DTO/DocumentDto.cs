using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    public class DocumentDto
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }

        internal static DocumentDto MapFrom(Document document)
        {
            return new DocumentDto
            {
                ModelId = document.ModelId,
                TwinId = document.TwinId,
                Id = document.Id,
                Name = document.Name,
                Uri = document.Uri,
            };
        }

        internal static List<DocumentDto> MapFrom(List<Document> output)
        {
            return output.Select(MapFrom).ToList();
        }
    }
}
