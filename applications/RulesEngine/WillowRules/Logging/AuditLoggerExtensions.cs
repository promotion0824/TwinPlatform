using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;

namespace WillowRules.Logging;

/// <summary>
/// Extends <see cref="IAuditLogger"/>
/// </summary>
public static class AuditLoggerExtensions
{
	/// <summary>
	/// Log an informational Audit event.
	/// </summary>
	/// <param name="auditLogger">The <see cref="IAuditLogger"/> through which the audit message will be recorded.</param>
	/// <param name="user">A <see cref="ClaimsPrincipal"/> from which to derive a user identifier based on Claims. Will
	/// look for <see cref="ClaimTypes.Email"/> and <see cref="ClaimTypes.NameIdentifier"/> in that order. If neither
	/// is found, "NO USER" will be used.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	public static void LogInformation(this IAuditLogger auditLogger, ClaimsPrincipal user, string? messageTemplate, params object[] args)
	{
		auditLogger.LogInformation(
			userId: user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "NO USER",
			messageTemplate,
			args);
	}

	/// <summary>
	/// Log an informational Audit event.
	/// </summary>
	/// <param name="auditLogger">The <see cref="IAuditLogger"/> through which the audit message will be recorded.</param>
	/// <param name="user">A <see cref="ClaimsPrincipal"/> from which to derive a user identifier based on Claims. Will
	/// look for <see cref="ClaimTypes.Email"/> and <see cref="ClaimTypes.NameIdentifier"/> in that order. If neither
	/// is found, "NO USER" will be used.</param>
	/// <param name="scope">Additional custom properties that will be included in the log message scope.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	public static void LogInformation(this IAuditLogger auditLogger, ClaimsPrincipal user, Dictionary<string, object> scope, string? messageTemplate, params object[] args)
	{
		using (auditLogger.BeginScope(scope))
		{
			LogInformation(auditLogger, user, messageTemplate, args);
		}
	}

	/// <summary>
	/// Log an informational Audit event.
	/// </summary>
	/// <param name="auditLogger">The <see cref="IAuditLogger"/> through which the audit message will be recorded.</param>
	/// <param name="userId">A user identifier that will be appended to the audit message.</param>
	/// <param name="scope">Additional custom properties that will be included in the log message scope.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	public static void LogInformation(this IAuditLogger auditLogger, string userId, Dictionary<string, object> scope, string? messageTemplate, params object[] args)
	{
		using (auditLogger.BeginScope(scope))
		{
			auditLogger.LogInformation(userId, messageTemplate, args);
		}
	}
}

