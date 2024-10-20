namespace DirectoryCore.Controllers.Requests
{
    public class SendContactRequestEmailRequest
    {
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}
