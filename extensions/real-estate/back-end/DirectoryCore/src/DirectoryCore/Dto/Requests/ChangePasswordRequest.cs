namespace DirectoryCore.Dto.Requests
{
    public class ChangePasswordRequest
    {
        public string Password { get; set; }
        public string EmailToken { get; set; }
    }
}
