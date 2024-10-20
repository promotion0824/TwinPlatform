// POCO class
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace RulesEngine.Web
{
    /// <summary>
    /// A <see cref="RuleInstance"/> DTO
    /// </summary>
    public class RuleInstanceDto
    {
        /// <summary>
        /// Creates a new <see cref="RuleInstanceDto" />
        /// </summary>
        public RuleInstanceDto(RuleInstance ri, RuleInstanceMetadata metadata, bool canViewRule)
        {
            this.Id = ri.Id;
            this.RuleId = ri.RuleId;
            this.RuleName = ri.RuleName;
            this.Description = ri.Description;
            this.Recommendations = ri.Recommendations;
            this.RuleTemplate = ri.RuleTemplate;
            this.EquipmentId = ri.EquipmentId;
            this.EquipmentName = ri.EquipmentName;
            this.EquipmentUniqueId = ri.EquipmentUniqueId;
            this.SiteId = ri.SiteId;
            this.Valid = ri.Status == RuleInstanceStatus.Valid;
            this.Status = ri.Status;
            this.Disabled = ri.Disabled;

            if (canViewRule)
            {
                this.RuleParametersBound = ri.RuleParametersBound?.Select(rp => new RuleParameterBoundDto(rp)).ToArray() ?? [];
                this.RuleImpactScoresBound = ri.RuleImpactScoresBound?.Select(rp => new RuleParameterBoundDto(rp)).ToArray() ?? [];
                this.RuleFiltersBound = ri.RuleFiltersBound?.Select(rp => new RuleParameterBoundDto(rp)).ToArray() ?? [];
                this.Parameters = ri.RuleParametersBound?.Select(p => new RuleParameterDto(p)).ToArray() ?? [];
                this.ImpactScores = ri.RuleImpactScoresBound?.Select(p => new RuleParameterDto(p)).ToArray() ?? [];
                this.Filters = ri.RuleFiltersBound?.Select(p => new RuleParameterDto(p)).ToArray() ?? [];
            }

            this.PointEntityIds = ri.PointEntityIds?.Select(rp => new NamedPointDto(rp)).ToArray() ?? [];
            this.RuleDependenciesBound = ri.RuleDependenciesBound?.Select(p => new RuleDependencyBoundDto(p)).ToArray() ?? [];
            this.RuleTriggersBound = ri.RuleTriggersBound?.Select(p => new RuleTriggerBoundDto(p)).ToArray() ?? [];
            this.ruleDependencyCount = ri.RuleDependencyCount;
            this.TriggerCount = metadata?.TriggerCount ?? 0;
            this.Locations = ri.TwinLocations?.ToArray() ?? [];
            this.Feeds = ri.Feeds;
            this.FedBy = ri.FedBy;
            this.OutputTrendId = ri.OutputTrendId;
            this.TimeZone = ri.TimeZone;
            this.IsCalculatedPointTwin = ri.RuleTemplate == RuleTemplateCalculatedPoint.ID;
            this.CapabilityCount = ri.CapabilityCount;
            this.ReviewStatus = metadata?.ReviewStatus ?? ReviewStatus.NotReviewed;
            this.Comments = metadata?.Comments?.OrderBy(v => v.Created).Select(rp => new RuleCommentDto(rp)).ToArray() ?? [];
            this.Tags = metadata?.Tags?.ToArray() ?? [];
            this.TotalComments = metadata?.TotalComments ?? 0;
            this.LastCommentPosted = metadata?.LastCommentPosted > DateTimeOffset.MinValue ? metadata?.LastCommentPosted : null;

            var allCollections = new[] { RuleParametersBound, RuleImpactScoresBound, RuleFiltersBound }
                .Concat(RuleTriggersBound?.Select(rtb => new[] { rtb.Condition, rtb.Value, rtb.Point }));

            allCollections.SelectMany(collection => collection).ToList().ForEach(rpDto =>
            {
                rpDto.PointExpressionExplained = ri.PointEntityIds?.Aggregate(
                    rpDto.PointExpression, (current, mapping) => current.Replace(mapping.Id, mapping.FullName)
                );
            });
        }

        /// <summary>
        /// Id of the rule instance (a combination of the rule Id and the twin id)
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Id of the rule that generated this instance
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Name of the rule
        /// </summary>
        public string RuleName { get; }

        /// <summary>
        /// Used to keep the descripiton of the rule and any equipment bound properties, e.g. {this.fanSpeed}
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Rule recommendations
        /// </summary>
        /// <remarks>
        /// This is copied from the Rule at the time the Insight is creeated (denormalized)
        /// </remarks>
        public string Recommendations { get; set; }

        /// <summary>
        /// Template to use for executing the rule
        /// </summary>
        public string RuleTemplate { get; }

        /// <summary>
        /// Anchor equipment twin id
        /// </summary>
        public string EquipmentId { get; }

        /// <summary>
        /// Anchor equipment twin name
        /// </summary>
        public string EquipmentName { get; }

        /// <summary>
        /// Anchor equipment unique Id
        /// </summary>
        public Guid? EquipmentUniqueId { get; }

        /// <summary>
        /// Site Id of the anchor equipment
        /// </summary>
        public Guid? SiteId { get; }

        /// <summary>
        /// Calculated property based on status
        /// </summary>
        public bool Valid { get; }

        /// <summary>
        /// Indicates validity status - If not valid then we were unable to bind all the variables to the equipment
        /// </summary>
        public RuleInstanceStatus Status { get; }

        /// <summary>
        /// Rule instance is disabled
        /// </summary>
        public bool Disabled { get; }

        /// <summary>
        /// The trend Ids that this instance is looking for in the real time data
        /// </summary>
        public NamedPointDto[] PointEntityIds { get; }

        /// <summary>
        /// Rule parameters and their binding to trend ids
        /// </summary>
        public RuleParameterBoundDto[] RuleParametersBound { get; } = [];

        /// <summary>
        /// Rule impact scores
        /// </summary>
        public RuleParameterBoundDto[] RuleImpactScoresBound { get; } = [];

        /// <summary>
        /// Rule filters bound
        /// </summary>
        public RuleParameterBoundDto[] RuleFiltersBound { get; } = [];

        /// <summary>
        /// Rule parameters
        /// </summary>
        public RuleParameterDto[] Parameters { get; } = [];

        /// <summary>
        /// Rule impact scores
        /// </summary>
        public RuleParameterDto[] ImpactScores { get; } = [];

        /// <summary>
        /// Rule filters
        /// </summary>
        public RuleParameterDto[] Filters { get; } = [];

        /// <summary>
        /// Rule dependencies
        /// </summary>
        public RuleDependencyBoundDto[] RuleDependenciesBound { get; }

        /// <summary>
        /// Rule triggers
        /// </summary>
        public RuleTriggerBoundDto[] RuleTriggersBound { get; }

        /// <summary>
        /// Total Rule Dependencies
        /// </summary>
        public int ruleDependencyCount { get; }

        /// <summary>
        /// Count of triggers for this rule instance
        /// </summary>
        public int TriggerCount { get; }

        /// <summary>
        /// Ascendant locations
        /// </summary>
        public TwinLocation[] Locations { get; }

        /// <summary>
        /// Entities that feed into the core rule entity
        /// </summary>
        public IList<string> Feeds { get; }

        /// <summary>
        /// Entities fed by the core rule entity
        /// </summary>
        public IList<string> FedBy { get; }

        /// <summary>
        /// The output trendId for a calculate point
        /// </summary>
        public string OutputTrendId { get; }

        /// <summary>
        /// The timezone of the primary equipment in the rule
        /// </summary>
        public string TimeZone { get; }

        /// <summary>
        /// Indicator that this instance is used for Calculated Point Twin
        /// </summary>
        public bool IsCalculatedPointTwin { get; set; }

        /// <summary>
        /// Total number of capabilities linked to the equipment
        /// </summary>
        public int CapabilityCount { get; set; }

        /// <summary>
        /// Review status used of the rule instance
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }

        /// <summary>
        /// Comments for this rule instance
        /// </summary>
        public RuleCommentDto[] Comments { get; set; } = [];

        /// <summary>
        /// When the last comment was posted
        /// </summary>
        public DateTimeOffset? LastCommentPosted { get; set; }

        /// <summary>
        /// Total comments posted
        /// </summary>
        public int TotalComments { get; set; }

        /// <summary>
        /// Tags
        /// </summary>
        public string[] Tags { get; init; } = [];
    }
}
