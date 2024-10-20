using System;
using Microsoft.AspNetCore.Authorization;

namespace RulesEngine.Web;

/// <summary>
/// An authorization handler for Willow permissions
/// </summary>
public interface IWillowAuthorizationHandler : IAuthorizationHandler
{
	/// <summary>
	/// The dot net type of the permission requirement
	/// </summary>
	Type RequirementType { get; }
}
