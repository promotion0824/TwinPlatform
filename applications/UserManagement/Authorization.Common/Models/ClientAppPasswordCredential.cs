namespace Authorization.Common.Models;

/// <summary>
/// Client Application Credential
/// </summary>
/// <param name="Name">Display Name of the Credential record.</param>
/// <param name="SecretText">Password.</param>
/// <param name="StartTime">Start Time the password is valid.</param>
/// <param name="EndTime">End Time till then the password is valid.</param>
public record ClientAppPasswordCredential (string Name, string SecretText, DateTimeOffset StartTime,  DateTimeOffset EndTime);
