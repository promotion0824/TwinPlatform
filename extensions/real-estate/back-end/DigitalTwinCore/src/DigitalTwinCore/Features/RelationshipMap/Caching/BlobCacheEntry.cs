using System;

namespace DigitalTwinCore.Features.RelationshipMap.Caching
{
    public interface IBlobCacheEntry
    {
        TimeSpan Expiration { get; set; }
    }

    public class BlobCacheEntry : IBlobCacheEntry
    {
        public TimeSpan Expiration { get; set; }
    }
}