using System.ComponentModel.DataAnnotations;

namespace Willow.Model.Requests
{
    public class UpgradeModelsRepoRequest
    {
        /// <summary>
        /// Repository owner
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string? Owner { get; set; }
        /// <summary>
        /// Repository name
        /// </summary>

        [Required(AllowEmptyStrings = false)]
        public string? Repository { get; set; }

        /// <summary>
		/// Optional repository tag or commit
		/// </summary>
		public string? Ref { get; set; }

        /// <summary>
		/// Path to the folder where ontology is stored
		/// </summary>
		[Required(AllowEmptyStrings = false)]
        public string? Path { get; set; }

        /// <summary>
		/// Submodules paths in the repository [Case sensite]
		/// </summary>
		public IEnumerable<string>? Submodules { get; set; }
    }
}
