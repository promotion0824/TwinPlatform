namespace Authorization.Common.Enums;

/// <summary>
/// Key names of different Cache Store used by authorization api.
/// </summary>
/// <remarks>
/// Make sure the enum definition and order matches the SDK Authorization.Common\Enums\CacheStoreType.cs
/// </remarks>
public enum CacheStoreType
{
    PermissionByUMAssignment,
    PermissionByADGroup
}
