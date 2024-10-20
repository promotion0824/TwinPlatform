using System;

namespace DirectoryCore.Controllers.Requests
{
    public class SendEmailRequest
    {
        public Guid[] ToCustomerUserIds { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}
