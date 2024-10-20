namespace Willow.Api.Client.Sdk.Directory.Dto;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is a valid suffix in this context.")]
public record CurrentUserDto(string Identity, CurrentUser? User = null);

public record CurrentUser(string? FirstName, string? LastName, IEnumerable<string> PlatformRoles, IEnumerable<EnvironmentPermission> EnvironmentPermissions);

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is a valid use case for this.")]
public record EnvironmentPermission(string CustomerCode, string EnvironmentCode);
