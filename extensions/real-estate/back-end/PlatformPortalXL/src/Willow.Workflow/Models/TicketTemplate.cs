using System;
using System.Collections.Generic;

using Willow.Calendar;

namespace Willow.Workflow
{
    public class TicketTemplate : Assignable
    {
        public Guid         Id               { get; set; }
        public Guid         CustomerId       { get; set; }
        public Guid         SiteId           { get; set; }
        public string       FloorCode        { get; set; }
        public string       SequenceNumber   { get; set; }
        public int          Priority         { get; set; }
        public TicketStatus Status           { get; set; }
        public string       Summary          { get; set; }
        public string       Description      { get; set; }
        
        public Guid?        ReporterId       { get; set; }
        public string       ReporterName     { get; set; }
        public string       ReporterPhone    { get; set; }
        public string       ReporterEmail    { get; set; }
        public string       ReporterCompany  { get; set; }
        
        public DateTime     CreatedDate      { get; set; }
        public DateTime     UpdatedDate      { get; set; }       
        public DateTime?    ClosedDate       { get; set; }

        public TicketSourceType SourceType   { get; set; }
        public EventDto     Recurrence       { get; set; }
        public string       NextTicketDate   { get; set; }
        public Duration     OverdueThreshold { get; set; }
        public string       Category         { get; set; }
        public Guid?        CategoryId       { get; set; }        
        
        public TicketAssignee Assignee { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Attachment> Attachments { get; set; }

        public List<TicketAsset> Assets   { get; set; }
        public List<TicketTwin> Twins { get; set; }
        public List<TicketTaskTemplate> Tasks { get; set; }
        public DataValue        DataValue { get; set; }
    }
}
