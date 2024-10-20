using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public AttachmentType Type { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }

        public string PreviewUrl { get; set; }
        public string Url { get; set; }

        public static AttachmentDto MapFromModel(Attachment model, IImageUrlHelper helper)
        {
            return new AttachmentDto
            {
                Id = model.Id,
                Type = model.Type,
                FileName = model.FileName,
                CreatedDate = model.CreatedDate,

                PreviewUrl = model.Path == null ? null : helper.GetAttachmentPreviewUrl(model.Path, model.Id),
                Url = model.Path == null ? null : helper.GetAttachmentUrl(model.Path, model.Id),
            };
        }

        public static List<AttachmentDto> MapFromModels(List<Attachment> models, IImageUrlHelper helper)
        {
            return models?.Select(x => MapFromModel(x, helper)).ToList();
        }
    }
}
