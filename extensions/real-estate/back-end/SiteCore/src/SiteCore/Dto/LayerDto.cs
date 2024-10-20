using System;
using SiteCore.Domain;

namespace SiteCore.Dto
{
    public class LayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TagName { get; set; }

        public static LayerDto MapFrom(Layer layer)
        {
            if (layer == null)
            {
                return null;
            }

            return new LayerDto
            {
                Id = layer.Id,
                Name = layer.Name,
                TagName = layer.TagName,
            };
        }
    }
}
