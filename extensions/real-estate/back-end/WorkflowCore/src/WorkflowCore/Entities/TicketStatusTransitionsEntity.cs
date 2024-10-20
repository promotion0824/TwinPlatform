using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities;

[Table("WF_TicketStatusTransitions")]
public class TicketStatusTransitionsEntity
{
    public Guid Id { get; set; }
    public int FromStatus { get; set; }
    public int ToStatus { get; set; }
}

