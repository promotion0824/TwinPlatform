using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalTwinCore.Constants;

namespace DigitalTwinCore.Services
{
    public interface ITagMapperService
    {
        List<Tag> MapTags(Guid siteId, string modelId, IDictionary<string, object> input);
    }

    public class TagMapperService : ITagMapperService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly DigitalTwinDbContext _context;

        public TagMapperService(
            IMemoryCache memoryCache,
            DigitalTwinDbContext context)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        private Dictionary<string, TagEntity> TagEntities => _memoryCache.GetOrCreate(Properties.Tags, (c) => {
            c.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
            // Used in multithreaded environment, create new context for every Task.
            using var dbContext = new DigitalTwinDbContext(_context.Options);
            return dbContext.Tags.ToDictionary(c => c.Name.ToUpperInvariant());
        });

        private List<SiteVirtualTag> GetVirtualTagEntities(Guid siteId)
        {
            return _memoryCache.GetOrCreate($"virtualtags_{siteId}", (c) => {
                c.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                // Used in multithreaded environment, create new context for every Task.
                using var dbContext = new DigitalTwinDbContext(_context.Options);
                return SiteVirtualTag.MapFrom(dbContext.VirtualTags.AsQueryable().Where(t => t.SiteId == siteId));
            });
        }


        public List<Tag> MapTags(Guid siteId, string modelId, IDictionary<string, object> tagNames)
        {
			if (tagNames == null)
			{
				return new List<Tag>();
			}

			var tagEntities = TagEntities;

			var output = tagNames.Keys.Select(name => new Tag
			{
				Name = name,
				Type = tagEntities.ContainsKey(name.ToUpperInvariant()) ?
						(TagType)tagEntities[name.ToUpperInvariant()].TagType :
						TagType.General
			}).OrderBy(t => t.Name).ToList();

			var virtualTagEntities = GetVirtualTagEntities(siteId);
			output.AddRange(
				virtualTagEntities
				.Where(vt => vt.IsMatchFor(modelId, output) && !output.Select(t => t.Name).Contains(vt.Tag))
				.Select(vt => new Tag
				{
					Name = vt.Tag,
					Type = (TagType)tagEntities[vt.Tag.ToUpperInvariant()].TagType
				}));

			return output.OrderBy(t => t.Name).ToList();
		}
    }
}
