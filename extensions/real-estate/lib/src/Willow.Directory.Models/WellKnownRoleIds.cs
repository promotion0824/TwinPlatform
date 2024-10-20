using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Directory.Models
{
    public static class WellKnownRoleIds
    {
        public static readonly Guid CustomerAdmin   = Guid.Parse("48174c3b-57ed-4d0d-badc-6c9d8d0afab1");
        public static readonly Guid PortfolioAdmin  = Guid.Parse("622723ae-ded8-45aa-a7fb-0d2d307ee65d");
        public static readonly Guid SiteAdmin       = Guid.Parse("798edb9e-df4b-4398-a19b-2d2cff006cd4");
        public static readonly Guid PortfolioViewer = Guid.Parse("F652E84E-3CA9-4E74-8EC9-7FD337B17B47");
        public static readonly Guid SiteViewer      = Guid.Parse("95DA3F2F-5E36-4619-9FD8-EB0094B9F16C");
    }
}
