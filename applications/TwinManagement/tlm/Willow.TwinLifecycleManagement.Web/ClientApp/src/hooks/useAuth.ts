import { useContext } from "react";
import { AuthorizationPermissionContext } from "../context/AuthorizationPermissionContext";

/**
 * AuthLogic used to determine hasPermission condition
 *  - All: All requiredPermissions must be present
 *  - Any: At least one requiredPermissions must be present
 */
export enum AuthLogic {
  All = "All",
  Any = "Any"
}

export const useAuth = () => {

  const authContext = useContext(AuthorizationPermissionContext);
  const isLoadingPermission = () => { return authContext.isLoading };
  const hasPermission = (requiredPermissions: string[] | null, authLogic: AuthLogic = AuthLogic.All): boolean => {

    return !authContext.isLoading && (requiredPermissions === null
      ||
      (AuthLogic.All === authLogic && requiredPermissions.every(p => authContext.response.permissions != null && authContext.response.permissions.indexOf(p) !== -1))
      ||
      (AuthLogic.Any === authLogic && requiredPermissions.filter(p => authContext.response.permissions != null && authContext.response.permissions.indexOf(p) !== -1).length > 0)
      ||
      authContext.response.isAdminUser
      ||
      false
    );
  };

  return { hasPermission, isLoading: isLoadingPermission };
};

