using System;
using System.Collections.Generic;
using Willow.Rules.Model;

// POCO class
#nullable disable

namespace RulesEngine.Web
{
    /// <summary>
    /// Equipment item and associated rule instances
    /// </summary>
    public class EquipmentDto
    {
        /// <summary>
        /// Id for the equipment, currently just the one
        /// </summary>
        public string EquipmentId { get; set; }

        /// <summary>
        /// ModelId for the equipment, currently just the one
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Name of the equipment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the equipment
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Related entities up
        /// </summary>
        public RelatedEntityDto[] RelatedEntities { get; set; }

        /// <summary>
        /// Related entities down
        /// </summary>
        public RelatedEntityDto[] InverseRelatedEntities { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        public CapabilityDto[] Capabilities { get; set; }

        /// <summary>
        /// Tags
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Unit of measure (for capabilities)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Interval that we expect this trend to follow (in seconds)
        /// </summary>
        public int? TrendInterval { get; set; }

        /// <summary>
        /// Value Expression
        /// </summary>
        public string ValueExpression { get; set; }

        /// <summary>
        /// Legacy Guid (as string) siteId
        /// </summary>
        public Guid? SiteId { get; set; }

        /// <summary>
        /// Legacy Guid equipment unique id
        /// </summary>
        public Guid? EquipmentUniqueId { get; set; }

        /// <summary>
        /// External Id used in some time series values
        /// </summary>
        /// <remarks>
        /// If there's a trendId we match on that otherwise we match on connector Id + external Id
        /// </remarks>
        public string ExternalId { get; set; }

        /// <summary>
        /// Connector Id used in some time series values
        /// </summary>
        public string ConnectorId { get; set; }

        /// <summary>
        /// Trend Id used in some time series values
        /// </summary>
        /// <remarks>
        /// If there's a trendId we match on that otherwise we match on connector Id + external Id
        /// </remarks>
        public string TrendId { get; set; }

        /// <summary>
        /// Timezone
        /// </summary>
        public string Timezone { get; set; }

        /// <summary>
        /// Equipment is calculated point
        /// </summary>
        public bool IsCalculatedPointTwin { get; set; }

        /// <summary>
        /// Current validation status for a capability twin
        /// </summary>
        public TimeSeriesStatus? CapabilityStatus { get; set; }

        /// <summary>
        /// Additional properties
        /// </summary>
        public Dictionary<string, object> Contents { get; set; }

        /// <summary>
        /// The date and time the twin was last updated in cache
        /// </summary>
        public DateTimeOffset? LastUpdatedOn { get; set; }

        /// <summary>
        /// Model properties 
        /// </summary>
        public ModelPropertyDto[] Properties { get; set; } = [];

        /// <summary>
        /// Ascendant locations
        /// </summary>
        public TwinLocation[] Locations { get; set; } = [];
    }
}


/// <summary>
/// A twin property
/// </summary>
public class TwinPropertyDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public TwinPropertyDto(string modelId, string propertyName, object value)
    {
        this.ModelId = modelId;
        this.PropertyName = propertyName;
        this.PropertyValue = value;
    }

    /// <summary>
    /// From which model id
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Name of property
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Property value
    /// </summary>
    public object PropertyValue { get; set; }
}
