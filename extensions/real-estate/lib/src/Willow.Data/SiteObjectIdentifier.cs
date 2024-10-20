using System;

namespace Willow.Data
{
    public class SiteObjectIdentifier
    {
        public Guid SiteId { get; set; }
        public Guid Id     { get; set; }

        public SiteObjectIdentifier(Guid siteId, Guid id)
        {
            this.SiteId = siteId;
            this.Id = id;
        }

        public override string ToString()
        {
            return SiteId.ToString() + "_" + Id.ToString();
        }
    }
}
