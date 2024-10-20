using System.ComponentModel.DataAnnotations;

namespace Willow.Model.Requests
{
    public class GitRepoRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string? FolderPath { get; set; }

        public string? BranchRef { get; set; }

        public string? UserInfo { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? UserId { get; set; }
    }
}
