using System;
using System.Collections.Generic;
using WorkflowCore.Models;

using Willow.Calendar;
using WorkflowCore.Controllers.Request.Validation;
using WorkflowCore.Dto;

namespace WorkflowCore.Controllers.Request
{
    public class CreateTicketTemplateRequest
    {
        public Guid         CustomerId           { get; set; }
        public string       FloorCode            { get; set; }
        public string       SequenceNumberPrefix { get; set; }
        public int          Priority             { get; set; }
        public int          Status               { get; set; }
        public string       Summary              { get; set; }
        public string       Description          { get; set; }
        public Guid?        ReporterId           { get; set; }
        public string       ReporterName         { get; set; }
        public string       ReporterPhone        { get; set; }
        public string       ReporterEmail        { get; set; }
        public string       ReporterCompany      { get; set; }
        [RequiredAssigneeType("AssigneeId")]
        public AssigneeType AssigneeType         { get; set; }
        public Guid?        AssigneeId           { get; set; }
        public SourceType   SourceType           { get; set; }
        public Guid?        SourceId             { get; set; }
        public Guid?        CategoryId           { get; set; }
        public EventDto     Recurrence           { get; set; }
        public Duration     OverdueThreshold     { get; set; }
        public List<TicketAsset> Assets          { get; set; }
        public List<TicketTwin> Twins            { get; set; }
        public List<TicketTaskTemplate> Tasks    { get; set; }
        public DataValue    DataValue            { get; set; }
    }
}
