using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsightCore.Entities;

[Table("InsightLocations")]
[PrimaryKey(nameof(LocationId), nameof(InsightId))]
public class InsightLocationEntity
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(250)]
    public string LocationId { get; set; }
    [Required]
    public Guid InsightId { get; set; }
    [ForeignKey(nameof(InsightId))]
    public InsightEntity Insight { get; set; }
}
