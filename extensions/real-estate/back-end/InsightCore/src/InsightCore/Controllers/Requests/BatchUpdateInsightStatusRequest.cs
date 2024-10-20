using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InsightCore.Models;

namespace InsightCore.Controllers.Requests;

public class BatchUpdateInsightStatusRequest
{
    public List<Guid> Ids { get; set; }

    [Required(ErrorMessage = "the insight status is required")]
    public InsightStatus? Status { get; set; }

    public string Reason { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public Guid? SourceId { get; set; }
}
