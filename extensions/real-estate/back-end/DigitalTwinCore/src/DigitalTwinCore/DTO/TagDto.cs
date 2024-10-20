using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;
using DigitalTwinCore.Constants;

namespace DigitalTwinCore.Dto
{
    public class TagDto
    {
        public string Name { get; set; }

        public string Feature { get; set; }

        internal static TagDto MapFrom(Tag tag)
        {
            return tag switch
            {
                null => null,
                _ => new TagDto
                {
                    Name = tag.Name,
                    Feature = tag.Type switch
                    {
                        TagType.TwoD => Dtos.Tags.TwoD,
                        TagType.ThreeD => Dtos.Tags.ThreeD,
                        _ => null
                    }
                }
            };
        }

        internal static List<TagDto> MapFrom(List<Tag> tags)
        {
            if (tags == null)
            {
                return new List<TagDto>();
            }

            return tags.Select(MapFrom).ToList();
        }
    }
}
