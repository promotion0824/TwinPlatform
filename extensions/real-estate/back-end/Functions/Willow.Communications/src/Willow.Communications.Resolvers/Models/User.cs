using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Willow.Directory.Models;

namespace Willow.Communications.Resolvers.Models;

public interface IUser
{
    public Guid Id { get; }
    public string Name { get; }
    public UserType Type { get; }
    public string Email { get; }
    public string Mobile { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Initials { get; }
    public string Language { get; }
    public UserStatus Status { get; }
}

public abstract class BaseUser : IUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Initials { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public string Email { get; set; } = "";
    public string Mobile { get; set; } = "";
    public UserStatus Status { get; set; }
    public string Auth0UserId { get; set; } = "";
    public string Language { get; set; } = "";

    public string Name => ((FirstName ?? "") + " " + (LastName ?? "")).Trim();
    public abstract UserType Type { get; }
}

public class User : BaseUser
{
    public string Company { get; set; } = "";
    public Guid CustomerId { get; set; }
    public override UserType Type => UserType.Customer;
}

public enum UserStatus
{
    Pending = -1,
    Deleted = 0,
    Active = 1,
    Inactive = 2
}


