using System.ComponentModel.DataAnnotations;

namespace Willow.Model.Requests
{
    public class QueryTwinsRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string? Query { get; set; }

        public bool IncludeRelationships { get; set; }

        public bool IncludeIncomingRelationships { get; set; }

        public bool IdsOnly { get; set; }
    }
}
