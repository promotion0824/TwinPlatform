using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WorkflowCore.Services.MappedIntegration.Models;



public class MappedWorkgroup
{
    public MappedWorkgroup()
    {
        Assignees = Enumerable.Empty<MappedAssignee>().ToList();
    }
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public List<MappedAssignee> Assignees { get; set; }
}
