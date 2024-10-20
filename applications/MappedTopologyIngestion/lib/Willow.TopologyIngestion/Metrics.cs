// -----------------------------------------------------------------------
// <copyright file="Metrics.cs" company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion
{
    /// <summary>
    /// Metrics metadata for Application Insights.
    /// </summary>
    internal static class Metrics
    {
#pragma warning disable SA1600 // Elements should be documented
        public const string DefaultNamespace = "willow.topologyingestion";
        public const string ActionDimensionName = "Action";
        public const string ModelIdDimensionName = "ModelId";
        public const string StatusDimensionName = "Status";
        public const string TwinDimensionName = "Twin";
        public const string RelationshipTypeDimensionName = "RelationshipType";
        public const string InterfaceTypeDimensionName = "InterfaceType";
        public const string OutputDtmiTypeDimensionName = "OutputDtmi";
        public const string InputDtmiTypeDimensionName = "InputDtmi";

        public const string SucceededStatusDimension = "Succeeded";
        public const string FailedStatusDimension = "Failed";
        public const string ThrottledStatusDimension = "Throttled";
        public const string SkippedStatusDimension = "Skipped";
        public const string MappingPendingStatusDimension = "MappingPending";
        public const string MappingIgnoredStatusDimension = "MappingIgnored";

        public const string CreateActionDimension = "Create";
        public const string UpdateActionDimension = "Update";
        public const string MappingCreateActionDimension = "MappingCreate";
        public const string MappingUpdateActionDimension = "MappingUpdate";

        public const string IdDimensionName = "Id";
        public const string SiteDimensionName = "Site";
        public const string BuildingDimensionName = "Building";
        public const string IsSuccessDimensionName = "IsSuccess";
#pragma warning restore SA1600 // Elements should be documented
    }
}
