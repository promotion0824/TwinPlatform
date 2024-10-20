using Authorization.TwinPlatform.Common.Model;

namespace PlatformPortalXL.Auth.Extensions;

public static class AuthorizedPermissionExtensions
{
    public static bool IsGlobalAssignment(this AuthorizedPermission perm) => string.IsNullOrEmpty(perm.Expression);
}
