using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class TicketBase
    {
        public Guid             Id                  { get; set; }
        public Guid             CustomerId          { get; set; }
        public Guid             SiteId              { get; set; }
        public string           FloorCode           { get; set; }
        public string           SequenceNumber      { get; set; }
        public int              Priority            { get; set; }
        public int              Status              { get; set; }
        public string           Summary             { get; set; }
        public string           Description         { get; set; }
        public Guid?            ReporterId          { get; set; }
        public string           ReporterName        { get; set; }
        public string           ReporterPhone       { get; set; }
        public string           ReporterEmail       { get; set; }
        public string           ReporterCompany     { get; set; }
        public AssigneeType     AssigneeType        { get; set; }
        public Guid?            AssigneeId          { get; set; }
        public string           AssigneeName        { get; set; }
        public DateTime         CreatedDate         { get; set; }
        public DateTime         UpdatedDate         { get; set; }
        public DateTime?        ClosedDate          { get; set; }
        public SourceType       SourceType          { get; set; }

        public List<TicketAttachment> Attachments   { get; set; }
    }

    public class Ticket : TicketBase
    {
        public IssueType        IssueType                   { get; set; }
        public Guid?            IssueId                     { get; set; }
        public string           IssueName                   { get; set; }
        public Guid?            InsightId                   { get; set; }
        public string           InsightName                 { get; set; }
        public string           Cause                       { get; set; }
        public string           Solution                    { get; set; }
        public Guid             CreatorId                   { get; set; }
        public DateTime?        DueDate                     { get; set; }
        public DateTime?        StartedDate                 { get; set; }
        public DateTime?        ResolvedDate                { get; set; }
        public Guid?            SourceId                    { get; set; }
        public string           SourceName                  { get; set; }
        public string           ExternalId                  { get; set; }
        public string           ExternalStatus              { get; set; }
        public string           ExternalMetadata            { get; set; }
        public List<Comment>    Comments                    { get; set; }
        public Dictionary<string, string> CustomProperties  { get; set; }
        public List<string> ExtendableSearchablePropertyKeys { get; set; }
        public List<TicketTask> Tasks                       { get; set; }
        public TicketCategory   Category                    { get; set; }
        public Guid?            CategoryId                  { get; set; }
        public DateTime?        ExternalCreatedDate         { get; set; }
        public DateTime?        ExternalUpdatedDate         { get; set; }
        public bool             LastUpdatedByExternalSource { get; set; }
        public DateTime         ComputedCreatedDate         { get; set; }
        public DateTime         ComputedUpdatedDate         { get; set; }
        public decimal?          Latitude                    { get; set; }
        public decimal?          Longitude                   { get; set; }

        // Scheduled ticket
        public DateTime?        ScheduledDate               { get; set; }
        public int              Occurrence                  { get; set; } = 0;
        public Guid?            TemplateId                  { get; set; }
        public string           Notes                       { get; set; }
        public string TwinId { get; set; }
        public bool? CanResolveInsight { get; set; }
        public Guid? SubStatusId { get; set; }
        public Guid? JobTypeId { get; set; }
        public TicketJobType JobType { get; set; }
        public string SpaceTwinId { get; set; }
        public Guid? ServiceNeededId { get; set; }
        public TicketServiceNeeded ServiceNeeded { get; set; }
        public List<Insight> Diagnostics { get; set; }
        public List<int> NextValidStatus { get; set; }
    }
}
