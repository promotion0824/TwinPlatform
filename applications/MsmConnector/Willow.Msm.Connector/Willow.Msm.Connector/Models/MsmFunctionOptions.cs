namespace Willow.Msm.Connector.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents configuration options for an MSM function, specifying requirements that must be met for the function to execute properly.
    /// </summary>
    public class MsmFunctionOptions
    {
        /// <summary>
        /// Gets or sets a list of model identifiers that are required by the MSM function
        /// These identifiers specify which models must be available for the function's operation.
        /// </summary>
        public List<string> RequiredModelIds { get; set; } = new List<string>();
    }
}
