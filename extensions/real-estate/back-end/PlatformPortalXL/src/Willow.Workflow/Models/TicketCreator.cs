using System;
using Willow.Platform.Users;

namespace Willow.Workflow;

public class TicketCreator
{
	public Guid Id { get; set; }
	public UserType Type { get; set; }
	public string Email { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Mobile { get; set; }
	public string Company { get; set; }
	public string Initials { get; set; }
	public string Name => $"{FirstName} {LastName}";
	
}
