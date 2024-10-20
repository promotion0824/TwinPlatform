using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsightCore.Entities
{
    [Table("InsightNextNumber")]
    public class InsightNextNumberEntity
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(16)]
        public string Prefix { get; set; }
        public long NextNumber { get; set; }
    }
}
