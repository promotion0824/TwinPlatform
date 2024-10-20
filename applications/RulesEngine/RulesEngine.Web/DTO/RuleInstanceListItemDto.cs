// POCO class
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web
{
    /// <summary>
    /// A lite version of a rule instance
    /// </summary>
    public class RuleInstanceListItemDto
    {
        /// <summary>
        /// Creates a new <see cref="RuleInstanceListItemDto" />
        /// </summary>
        public RuleInstanceListItemDto(string id, string equipmentId, string equipmentName, RuleInstanceStatus status)
        {
            this.id = id;
            this.equipmentId = equipmentId;
            this.equipmentName = equipmentName;
            this.status = status;
        }

        /// <summary>
        /// Id of the rule instance (a combination of the rule Id and the twin id)
        /// </summary>
        public string id { get; }

        /// <summary>
        /// Anchor equipment twin id
        /// </summary>
        public string equipmentId { get; }

        /// <summary>
        /// Anchor equipment twin name
        /// </summary>
        public string equipmentName { get; }

        /// <summary>
        /// Calculated property based on status
        /// </summary>
        public RuleInstanceStatus status { get; }
    }
}
