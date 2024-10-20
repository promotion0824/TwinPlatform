﻿
using Willow.Platform.Models;

namespace Willow.Management
{
    public class PortfolioFeaturesDto
    {
        public bool IsKpiDashboardEnabled { get; set; }
        public bool IsTwinsSearchEnabled { get; set; }

        public static PortfolioFeaturesDto MapFrom(PortfolioFeatures model)
        {
            if (model == null)
            {
                model = new PortfolioFeatures();
            }

            return new PortfolioFeaturesDto
            {
                IsKpiDashboardEnabled = model.IsKpiDashboardEnabled,
                IsTwinsSearchEnabled = model.IsTwinsSearchEnabled
            };
        }
    }
}
