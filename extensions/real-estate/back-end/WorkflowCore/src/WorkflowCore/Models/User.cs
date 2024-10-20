using System;

namespace WorkflowCore.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
    }
}
