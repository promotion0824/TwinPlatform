using System;
using System.Threading.Tasks;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Services
{
    public interface IAttachmentsServices
    {
        Task<TicketAttachment> CreateTicketAttachment(Guid customerId, Guid siteId, Guid ticketId, string originalFileName, byte[] content);
        Task<AttachmentBase> CreateCheckRecordAttachment(Guid customerId, Guid siteId, Guid checkRecordId, string originalFileName, byte[] content);
        Task DeleteTicketAttachment(Guid customerId, Guid siteId, Guid ticketId, Guid attachmentId);
        Task DeleteCheckRecordAttachment(Guid customerId, Guid siteId, Guid checkRecordId, Guid attachmentId);
    }

    public class AttachmentsServices : IAttachmentsServices
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IWorkflowRepository _repository;
        private readonly IImageHubService _imageHub;
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IUserInspectionRepository _userInspectionRepository;

        public AttachmentsServices(IDateTimeService dateTimeService, IWorkflowRepository repository, IImageHubService imageHub, IImagePathHelper imagePathHelper, IUserInspectionRepository userInspectionRepository)
        {
            _dateTimeService = dateTimeService;
            _repository = repository;
            _imageHub = imageHub;
            _imagePathHelper = imagePathHelper;
            _userInspectionRepository = userInspectionRepository;
        }

        public async Task<TicketAttachment> CreateTicketAttachment(Guid customerId, Guid siteId, Guid ticketId, string originalFileName, byte[] content)
        {
            var path = _imagePathHelper.GetTicketAttachmentsPath(customerId, siteId, ticketId);
            var createdImageDescriptor = await _imageHub.CreateAttachment(path, originalFileName, content);
            var attachment = new TicketAttachment
            {
                Id = createdImageDescriptor.ImageId,
                TicketId = ticketId,
                Type = AttachmentType.Image,
                FileName = originalFileName,
                CreatedDate = _dateTimeService.UtcNow
            };
            await _repository.CreateAttachment(attachment);
            return attachment;
        }

        public async Task DeleteTicketAttachment(Guid customerId, Guid siteId, Guid ticketId, Guid attachmentId)
        {
            bool result = await _repository.DeleteAttachment(ticketId, attachmentId);
            if (!result)
            {
                throw new NotFoundException(new { AttachementId = attachmentId });
            }
            var path = _imagePathHelper.GetTicketAttachmentsPath(customerId, siteId, ticketId);
            await _imageHub.DeleteAttachment(path, attachmentId);
        }

        public async Task<AttachmentBase> CreateCheckRecordAttachment(Guid customerId, Guid siteId, Guid checkRecordId, string originalFileName, byte[] content)
        {
            var path = _imagePathHelper.GetCheckRecordAttachmentsPath(customerId, siteId, checkRecordId);
            var createdImageDescriptor = await _imageHub.CreateAttachment(path, originalFileName, content);
            var attachment = new AttachmentBase
            {
                Id = createdImageDescriptor.ImageId,
                Type = AttachmentType.Image,
                FileName = originalFileName,
                CreatedDate = _dateTimeService.UtcNow
            };

            await _userInspectionRepository.UpdateCheckRecordAttachments(checkRecordId, attachment);
            return attachment;
        }

        public async Task DeleteCheckRecordAttachment(Guid customerId, Guid siteId, Guid checkRecordId, Guid attachmentId)
        {
			await _userInspectionRepository.DeleteCheckRecordAttachments(checkRecordId, attachmentId);

			var path = _imagePathHelper.GetCheckRecordAttachmentsPath(customerId, siteId, checkRecordId);
			await _imageHub.DeleteAttachment(path, attachmentId);
		}
    }
}
