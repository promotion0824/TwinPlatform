using DigitalTwinCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Models
{
    public class SiteVirtualTag
    {
        public Guid Id { get; set; }

        public Guid SiteId { get; set; }

        public string Tag { get; set; }

        public string MatchModelId { get; set; }

        public List<string> MatchTagList { get; set; }

        public bool IsMatchFor(string modelId, IEnumerable<Tag> tags) => 
            (MatchModelId != null && modelId.Contains(MatchModelId)) || 
            (MatchModelId == null && MatchTagList.SequenceEqual(tags.Select(t => t.Name)));

        public static SiteVirtualTag MapFrom(SiteVirtualTagEntity entity) =>
            new SiteVirtualTag
            {
                Id = entity.Id,
                SiteId = entity.SiteId,
                Tag = entity.Tag,
                MatchModelId = entity.MatchModelId,
                MatchTagList = entity.MatchTagList?.Split(',').OrderBy(x => x).ToList()
            };

        public static List<SiteVirtualTag> MapFrom(IEnumerable<SiteVirtualTagEntity> entities) =>
            entities?.Select(MapFrom).ToList() ?? new List<SiteVirtualTag>();
    }
}
