using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Dto
{
    public class GenerateCheckRecordDto
    {
        public Guid  Id                  { get; set; }
        public Guid  LastRecordId        { get; set; }
        public CheckRecordStatus Status  { get; set; }
    }
}
