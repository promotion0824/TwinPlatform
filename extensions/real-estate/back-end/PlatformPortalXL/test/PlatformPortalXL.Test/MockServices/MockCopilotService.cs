using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Copilot;

namespace PlatformPortalXL.Test.MockServices
{
	public class MockCopilotService : ICopilotService
	{
        public async Task<CopilotChatResponse> ChatAsync(CopilotChatRequest request)
        {
            return new CopilotChatResponse()
            {
                 ResponseText = "ResponseText"
            };
        }

        public async Task<List<CopilotDocInfoResponse>> GetDocInfoAsync(CopilotDocInfoRequest request)
        {
            return [new CopilotDocInfoResponse()
            {
                Filename = "Test"
            }];
        }
    }
}
