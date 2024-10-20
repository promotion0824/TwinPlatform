using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Willow.Calendar;
using Willow.DataValidation;

namespace Willow.Workflow
{
    public class TicketTemplateRequestBase : TicketRequestBase
    {
	    [Required(AllowEmptyStrings = false, ErrorMessage = "Requestor name is required")]
	    [HtmlContent]
	    [StringLength(64)]
	    public string ReporterName { get; set; }

	    [Required(AllowEmptyStrings = false, ErrorMessage = "Contact number is required")]
	    [Phone(ErrorMessage = "Contact number is invalid")]
	    [StringLength(32)]
	    public string ReporterPhone { get; set; }

	    [Required(AllowEmptyStrings = false, ErrorMessage = "Contact email is required")]
	    [Email(ErrorMessage = "Contact email is invalid")]
	    [StringLength(64)]
	    public string ReporterEmail { get; set; }

	    public string Category { get; set; }

        [Required(ErrorMessage = "Recurrence is required")]
        public EventDto Recurrence { get; set; }

        [Required(ErrorMessage = "OverdueThreshold is required")]
        public Duration OverdueThreshold { get; set; }

        public List<TicketAsset> Assets { get; set; }
        public List<TicketTwin> Twins { get; set; }

        public List<TicketTaskTemplate> Tasks { get; set; }
    }

    public class CreateTicketTemplateRequest : TicketTemplateRequestBase
    {
        public Guid CustomerId { get; set; }
        public string SequenceNumberPrefix { get; set; }
        public TicketStatus Status { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public TicketSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public DataValue DataValue  { get; set; }
    }

    public class UpdateTicketTemplateRequest : TicketTemplateRequestBase
    {
        public TicketStatus? Status { get; set; }
        public bool ShouldUpdateReporterId { get; set; }
        public TicketAssigneeType? AssigneeType { get; set; }
        public List<Attachment> Attachments { get; set; }
        public DataValue DataValue { get; set; }
        public bool? PerformScheduleHitOnAddedAssets { get; set; }
    }

    public class WorkflowCreateTicketTemplateRequest : TicketTemplateRequestBase
    {
        public Guid CustomerId { get; set; }
        public string SequenceNumberPrefix { get; set; }
        public TicketStatus Status { get; set; }
        public TicketAssigneeType AssigneeType { get; set; }
        public TicketSourceType SourceType { get; set; }
        public DataValue DataValue { get; set; }
    }

    public class WorkflowUpdateTicketTemplateRequest : TicketTemplateRequestBase
    {
        public Guid CustomerId { get; set; }
        public TicketStatus? Status { get; set; }
        public bool ShouldUpdateReporterId { get; set; }
        public TicketAssigneeType? AssigneeType { get; set; }
        public List<Attachment> Attachments { get; set; }
        public bool? PerformScheduleHitOnAddedAssets { get; set; }
    }
}
