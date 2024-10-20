using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Willow.DataValidation;

namespace Willow.Workflow
{
    public class TicketRequestBase
    {
        [Range(1, 4, ErrorMessage = "Priority must be a value between 1 and 4")]
        public int Priority { get; set; }

        public string FloorCode { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Summary is required")]
        [HtmlContent]
        [StringLength(512)]
        public string Summary { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? AssigneeId { get; set; }

        public Guid? ReporterId { get; set; }

        [StringLength(64)]
        [HtmlContent]
        public string ReporterCompany { get; set; }

        public bool Template { get; set; }
        /// <summary>
        /// Ticket space twin id
        /// </summary>
        [StringLength(250)]
        public string SpaceTwinId { get; set; }
        /// <summary>
        /// Ticket job type id
        /// </summary>
        public Guid? JobTypeId { get; set; }
        /// <summary>
        /// Ticket service needed id
        /// </summary>
        public Guid? ServiceNeededId { get; set; }
    }

    public class TicketRequest : TicketRequestBase
    {
        public TicketIssueType    IssueType     { get; set; }
        public Guid?              IssueId       { get; set; }
        public TicketAssigneeType AssigneeType  { get; set; }

        public DateTime?          DueDate       { get; set; }

        [StringLength(250)]
        public string TwinId { get; set; }
    }

    public class CreateTicketRequest : TicketRequest
    {
        public Guid?              InsightId    { get; set; }
        public List<CreateTicketRequestInsight> Diagnostics { get; set; }
        public TicketSourceType?  SourceType   { get; set; }
        public string             Cause         { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Requestor name is required")]
        [HtmlContent]
        [StringLength(64)]
        public string ReporterName { get; set; }

        [Phone(ErrorMessage = "Contact number is invalid")]
        [StringLength(32)]
        public string ReporterPhone { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Contact email is required")]
        [Email(ErrorMessage = "Contact email is invalid")]
        [StringLength(64)]
        public string ReporterEmail { get; set; }
       
    }
    public class CreateTicketByScopeRequest : CreateTicketRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(250)]
        public string ScopeId { get; set; }

    }
    public class UpdateTicketRequest : TicketRequest
    {
        public int?               StatusCode    { get; set; }
        public string             Cause         { get; set; }
        public string             Solution      { get; set; }
        public string             Notes         { get; set; }
        public List<Guid>         AttachmentIds { get; set; }
        public List<TicketAsset>  Assets        { get; set; }
        public List<TicketTask>   Tasks         { get; set; }

        [HtmlContent]
        [StringLength(64)]
        public string ReporterName { get; set; }

        [Phone(ErrorMessage = "Contact number is invalid")]
        [StringLength(32)]
        public string ReporterPhone { get; set; }

        [Email(ErrorMessage = "Contact email is invalid")]
        [StringLength(64)]
        public string ReporterEmail { get; set; }
        /// <summary>
        /// Ticket sub status
        /// </summary>
        public Guid? SubStatusId { get; set; }
    }
    public class UpdateTicketByScopeRequest : UpdateTicketRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(250)]
        public string ScopeId { get; set; }

    }
    public class CreateTicketRequestInsight
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
