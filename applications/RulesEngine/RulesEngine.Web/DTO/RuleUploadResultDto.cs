using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Expressions.Visitor;
using Willow.Rules.Cache;
using Willow.Rules.Model;

// POCO class
#nullable disable

namespace RulesEngine.Web
{
    /// <summary>
    /// Rule upload result DTO for consumption by client-side code
    /// </summary>
    public class RuleUploadResultDto
    {
        /// <summary>
        /// List of failed file names
        /// </summary>
        public IEnumerable<string> Failures { get; set; } = Enumerable.Empty<string>();

        /// <summary>
		/// List of duplicate file names
		/// </summary>
		public IEnumerable<string> Duplicates { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Number of rules processed
        /// </summary>
        public int ProcessedCount { get; set; } = 0;

        /// <summary>
        /// Number of unique rules
        /// </summary>
        public int UniqueCount { get; set; } = 0;

        /// <summary>
        /// Number of duplicates
        /// </summary>
        public int DuplicateCount { get; set; } = 0;

        /// <summary>
        /// Number of failures
        /// </summary>
        public int FailureCount { get; set; } = 0;

        /// <summary>
        /// Overall status of process
        /// </summary>
        public bool Success { get; set; } = true;
    }
}
