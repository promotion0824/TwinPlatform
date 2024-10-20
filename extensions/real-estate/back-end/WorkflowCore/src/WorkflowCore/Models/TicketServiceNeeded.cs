using System;

namespace WorkflowCore.Models;

public class TicketServiceNeeded
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid?  CategoryId { get; set; }
}

