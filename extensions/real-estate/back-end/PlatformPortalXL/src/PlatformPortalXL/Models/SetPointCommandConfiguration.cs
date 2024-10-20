using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Models
{
    public class SetPointCommandConfiguration
    {
        public int Id { get; set; }
        public SetPointCommandType Type { get; set; }
        public string Description { get; set; }
        public string InsightName { get; set; }
        public string PointTags { get; set; }
        public string SetPointTags { get; set; }
        public decimal DesiredValueLimitation { get; set; }

        private List<string> PointTagsList =>
            PointTags.Split(',').ToList();

        private List<string> SetPointTagsList =>
            SetPointTags.Split(',').ToList();

        internal AssetPoint FindSetPoint(List<AssetPoint> points)
        {
            var pointsList = new List<AssetPoint>(points);
            foreach (var tag in SetPointTagsList)
            {
                pointsList = pointsList.Where(p => p.Tags.Any(t => t.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }

            return pointsList.Count == 1 ? pointsList.Single() : null;
        }

        internal AssetPoint FindPoint(List<AssetPoint> points)
        {
            var pointsList = new List<AssetPoint>(points);
            foreach (var tag in PointTagsList)
            {
                pointsList = pointsList.Where(p => p.Tags.Any(t => t.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }

            return pointsList.Count == 1 ? pointsList.Single() : null;
        }
    }
}
