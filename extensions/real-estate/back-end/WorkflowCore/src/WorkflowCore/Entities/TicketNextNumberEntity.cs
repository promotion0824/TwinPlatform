using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketNextNumber")]
    public class TicketNextNumberEntity
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(16)]
        public string Prefix { get; set; }
        public long NextNumber { get; set; }
    }
}
