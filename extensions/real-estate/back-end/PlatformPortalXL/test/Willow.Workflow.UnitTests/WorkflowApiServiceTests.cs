using System;
using Xunit;
using Moq;

using Willow.Api.Client;

namespace Willow.Workflow.UnitTests
{
    public class WorkflowApiServiceTests
    {
        private readonly Mock<IRestApi> _restApi = new Mock<IRestApi>();
        private readonly IWorkflowApiService _workflowApi;

        public WorkflowApiServiceTests()
        {
            _workflowApi = new WorkflowApiService(_restApi.Object);
        }

        [Fact]
        public void WorkflowApiService_CreateTicketTemplate_success()
        {
            var siteId = Guid.NewGuid();

            _workflowApi.CreateTicketTemplate(siteId, new WorkflowCreateTicketTemplateRequest { });

             _restApi.Verify( api=> api.Post<WorkflowCreateTicketTemplateRequest, TicketTemplate>($"sites/{siteId}/tickettemplate", It.IsAny<WorkflowCreateTicketTemplateRequest>(), null), Times.Once);
        }
    }
}
