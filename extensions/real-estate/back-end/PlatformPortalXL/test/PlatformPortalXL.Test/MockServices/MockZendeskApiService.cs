using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PlatformPortalXL.ServicesApi.ZendeskApi;

namespace PlatformPortalXL.Test.MockServices
{
	public class MockZendeskApiService : IZendeskApiService
	{
        public async Task<ZendeskTicketResponse> CreateTicket(ZendeskCreateTicketRequest zendeskTicketRequest)
        {
            return await Task.FromResult(new ZendeskTicketResponse
            {
                Ticket = new ZendeskTicket
                    { Description = zendeskTicketRequest.Comment.Body, Subject = zendeskTicketRequest.Subject }
            });
        }

        public async Task<List<ZendeskUpload>> UploadAttachmentsAsync(IFormFileCollection attachments)
        {
            return await Task.FromResult(new List<ZendeskUpload>());
        }

        public async Task<ZendeskUpload> UploadAttachmentAsync(
            IFormFile attachment, string token = null)
        {
            return await Task.FromResult(new ZendeskUpload { Attachments = new List<ZendeskAttachment>()
            {
                new ()
                {
                    FileName = attachment.FileName
                }
            } });
        }
	}
}
