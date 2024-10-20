using System;

namespace WorkflowCore.Services.Apis
{
    public interface IImagePathHelper
    {
        string GetTicketAttachmentsPath(Guid customerId, Guid siteId, Guid ticketId);
        string GetCheckRecordAttachmentsPath(Guid customerId, Guid siteId, Guid checkRecordId);
    }

    public class ImagePathHelper : IImagePathHelper
    {
        public string GetTicketAttachmentsPath(Guid customerId, Guid siteId, Guid ticketId)
        {
            return $"{customerId}/sites/{siteId}/tickets/{ticketId}";
        }

        public string GetCheckRecordAttachmentsPath(Guid customerId, Guid siteId, Guid checkRecordId)
        {
            return $"{customerId}/sites/{siteId}/checkRecords/{checkRecordId}";
        }
    }

}
