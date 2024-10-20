namespace DirectoryCore.Dto.Requests
{
    public class CreateCustomerUserRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string Company { get; set; }

        public bool UseB2C { get; set; }
    }
}
