using System;

namespace WorkflowCore.Models;

public class UserProfile
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Company { get; set; }
}

