using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;

using Newtonsoft.Json;

using Willow.Calendar;
using System.Linq;

namespace WorkflowCore.Entities
{
    [Table("WF_TicketTemplate")]
    public class TicketTemplateEntity
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Guid SiteId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string FloorCode { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(64)]
        public string SequenceNumber { get; set; }

        public int Priority { get; set; }

        public int Status { get; set; }

        [Required(AllowEmptyStrings = true)]
		[MaxLength(512)]
        public string Summary { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Description { get; set; }

        public Guid? ReporterId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string ReporterName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public string ReporterPhone { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string ReporterEmail { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(64)]
        public string ReporterCompany { get; set; }

        public AssigneeType AssigneeType { get; set; }

        public Guid? AssigneeId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public SourceType SourceType { get; set; }

        public string CategoryName { get; set; }
        public Guid? CategoryId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string OverdueThreshold { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Recurrence { get; set; }

        public string Attachments { get; set; }
        public string Assets { get; set; }
        public string Twins { get; set; }
        public string Tasks { get; set; }
        public string DataValue { get; set; }

        public static TicketTemplateEntity MapFromModel(TicketTemplate model)
        {
            if (model == null)
                return null;

            var assets      = model.Assets ?? new List<TicketAsset>();
            var twins       = model.Twins ?? new List<TicketTwin>();
            var tasks       = model.Tasks ?? new List<TicketTaskTemplate>();
            var attachments = model.Attachments ?? new List<TicketAttachment>();

            return new TicketTemplateEntity
            {
                Id               = model.Id,
                CustomerId       = model.CustomerId,
                SiteId           = model.SiteId,
                FloorCode        = model.FloorCode,
                SequenceNumber   = model.SequenceNumber,
                Priority         = model.Priority,
                Status           = model.Status,
                Summary          = model.Summary,
                Description      = model.Description,
                ReporterId       = model.ReporterId,
                ReporterName     = model.ReporterName,
                ReporterPhone    = model.ReporterPhone,
                ReporterEmail    = model.ReporterEmail,
                ReporterCompany  = model.ReporterCompany,
                AssigneeType     = model.AssigneeType,
                AssigneeId       = model.AssigneeId,
                CreatedDate      = model.CreatedDate,
                UpdatedDate      = model.UpdatedDate,
                ClosedDate       = model.ClosedDate,
                SourceType       = model.SourceType,
                CategoryName     = model.CategoryName,
                CategoryId       = model.CategoryId,
                Assets           = JsonConvert.SerializeObject(assets),
                Twins            = JsonConvert.SerializeObject(twins),
                Tasks            = JsonConvert.SerializeObject(tasks),
                Attachments      = JsonConvert.SerializeObject(attachments),
                Recurrence       = JsonConvert.SerializeObject(model.Recurrence),
                DataValue        = JsonConvert.SerializeObject(model.DataValue),

                OverdueThreshold = model.OverdueThreshold?.ToString() ?? ""
            };
        }

        public static IEnumerable<TicketTemplate> MapToModel(IEnumerable<TicketTemplateEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static TicketTemplate MapToModel(TicketTemplateEntity entity)
        {
            if (entity == null)
                return null;

            return new TicketTemplate
            {
                Id               = entity.Id,
                CustomerId       = entity.CustomerId,
                SiteId           = entity.SiteId,
                FloorCode        = entity.FloorCode,
                SequenceNumber   = entity.SequenceNumber,
                Priority         = entity.Priority,
                Status           = entity.Status,
                Summary          = entity.Summary,
                Description      = entity.Description,
                ReporterId       = entity.ReporterId,
                ReporterName     = entity.ReporterName,
                ReporterPhone    = entity.ReporterPhone,
                ReporterEmail    = entity.ReporterEmail,
                ReporterCompany  = entity.ReporterCompany,
                AssigneeType     = entity.AssigneeType,
                AssigneeId       = entity.AssigneeId,
                CreatedDate      = entity.CreatedDate,
                UpdatedDate      = entity.UpdatedDate,
                ClosedDate       = entity.ClosedDate,
                SourceType       = entity.SourceType,
                CategoryName     = entity.CategoryName,
                CategoryId       = entity.CategoryId,
                Attachments      = string.IsNullOrWhiteSpace(entity.Attachments) ? null : JsonConvert.DeserializeObject<List<TicketAttachment>>(entity.Attachments),
                DataValue        = string.IsNullOrWhiteSpace(entity.DataValue) ? null : JsonConvert.DeserializeObject<DataValue>(entity.DataValue),
                Assets           = string.IsNullOrWhiteSpace(entity.Assets) ? null : JsonConvert.DeserializeObject<List<TicketAsset>>(entity.Assets),
                Twins            = string.IsNullOrWhiteSpace(entity.Twins) ? null : JsonConvert.DeserializeObject<List<TicketTwin>>(entity.Twins),
                Tasks            = string.IsNullOrWhiteSpace(entity.Tasks) ? null : ConvertFromTaskJsonString(entity.Tasks),
                OverdueThreshold = new Duration(entity.OverdueThreshold),
                Recurrence       = string.IsNullOrWhiteSpace(entity.Recurrence) ? null : JsonConvert.DeserializeObject<Event>(entity.Recurrence),
            };
        }

        private static List<TicketTaskTemplate> ConvertFromTaskJsonString(string tasksJson)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<TicketTaskTemplate>>(tasksJson);
            }
            catch
            {
                var tasks = JsonConvert.DeserializeObject<string[]>(tasksJson);
                return tasks.Select(t => new TicketTaskTemplate { Description = t, Type = TaskType.Checkbox })
                            .ToList();
            }
        }
    }
}
