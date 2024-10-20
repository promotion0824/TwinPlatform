using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class TagDto
    {
        public string Name { get;set; }
        public string Feature { get; set; }
        public static TagDto MapFrom(Tag eTag)
        {
            if (eTag == null)
            {
                return null;
            }

            return new TagDto
            {
                Name = eTag.Name,
                Feature = eTag.Feature
            };
        }

        public static List<TagDto> MapFrom(IEnumerable<Tag> eTags)
        {
            return eTags?.Select(MapFrom).ToList();
        }
    }
}
