namespace Authorization.TwinPlatform.Permission.Api.Constants;

/// <summary>
/// Constants class for Telemetry Metric Dimensions
/// </summary>
public static class TelemetryMeterConstants
{
    public const string DatabaseMigrationSucceded = nameof(DatabaseMigrationSucceded);
    public const string DatabaseMigrationFailed = nameof(DatabaseMigrationFailed);

    public const string AdminPermissionRequests =nameof(AdminPermissionRequests);
    public const string InactiveUserRequests = nameof(InactiveUserRequests);

    public const string AutoImportPermissionRoles = nameof(AutoImportPermissionRoles);
}
