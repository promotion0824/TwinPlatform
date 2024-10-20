import { ReactNode } from "react";
import { useAuth } from "../Providers/PermissionProvider";

export enum AuthLogicOperator {
  all,
  any
}


export const AuthHandler = ({ children, requiredPermissions, authLogic =  AuthLogicOperator.all  }:
                            { children: ReactNode, requiredPermissions: string[], authLogic?: AuthLogicOperator}) => {

  const authData = useAuth();

  return (
    <>
      {(requiredPermissions.length === 0 ||
        (authLogic === AuthLogicOperator.all && requiredPermissions.every(p => authData.permissions.indexOf(p) !== -1)) ||
        (authLogic === AuthLogicOperator.any && requiredPermissions.some(p => authData.permissions.indexOf(p) !== -1)) ||
        authData.isAdminUser) ?
        children : null}
    </>
  );
}
