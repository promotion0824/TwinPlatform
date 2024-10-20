using System.ComponentModel.DataAnnotations;

namespace Authorization.Common.Models
{
    /// <summary>
    /// DTO Class that map to the Group Type Entity
    /// </summary>
    public class GroupTypeModel
    {
        /// <summary>
        /// Unique Identifier for the Group Type Model
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the Group Type
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;
    }
}
