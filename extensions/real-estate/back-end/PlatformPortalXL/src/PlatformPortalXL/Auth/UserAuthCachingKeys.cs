namespace PlatformPortalXL.Auth;

public static class UserAuthCachingKeys
{
    internal static string CacheKeyForAuthPolicyDecisions(string userId)
        => $"authz--policy-decisions-userId-[{userId}]";

    internal static string CacheKeyForGetUserPermissionsForApp(string userId, string extensionId)
        => $"authz-perms-for-app-userId-[{userId}]-extensionId-[{extensionId}";
}
