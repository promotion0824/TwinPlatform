using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Repository;
using WorkflowCore.Models;

namespace WorkflowCore.Services
{
    public interface INotificationReceiverService
    {
        Task<List<NotificationReceiver>> GetReceivers(Guid siteId);
    }

    public class NotificationReceiverService : INotificationReceiverService
    {
        private readonly IWorkflowRepository _repository;

        public NotificationReceiverService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<NotificationReceiver>> GetReceivers(Guid siteId)
        {
            return await _repository.GetNotificationReceivers(siteId);
        }
   }
}
