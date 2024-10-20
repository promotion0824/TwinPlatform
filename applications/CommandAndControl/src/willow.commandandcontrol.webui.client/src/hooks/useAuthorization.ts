import { useQuery } from "@tanstack/react-query";
import { ApiException, UserPermissionsResponseDto } from "../services/Clients";
import useApi from "./useApi";
import { useCallback } from "react";

export enum AppPermissions {
  canViewRequestsCommands = "CanViewRequestsCommands",
  canApproveExecute = "CanApproveExecute",
}

export default function useAuthorization() {
  const api = useApi();

  const { isLoading, data } = useQuery<UserPermissionsResponseDto, ApiException>({
    queryKey: ["authorization"],
    queryFn: () => api.getUserPermissions()
  });

  const { isAdminUser = false, permissions: usersPermissions = [] } =
    data || {};

  const hasPermission = useCallback(
    (permission: AppPermissions) => {
      return isAdminUser || usersPermissions.includes(permission);
    },
    [isAdminUser, usersPermissions]
  );

  return {
    isLoading,
    hasCanViewRequestsCommandsPermission: hasPermission(AppPermissions.canViewRequestsCommands),
    hasCanApproveExecutePermission: hasPermission(AppPermissions.canApproveExecute),
  };
}
