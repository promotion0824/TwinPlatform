import useAuthorization, { AppPermissions } from "../../hooks/useAuthorization";
import styled from "@emotion/styled";

type RequiredPermissions = {
  [key in keyof typeof AppPermissions]?: boolean | undefined;
};

interface AuthProps extends RequiredPermissions {
  // And the content we want to protect
  children: React.ReactNode;
}

/**
 * Hides children unless user have permissions
 * @param props
 */

export const VisibleIf = ({
  children,
  ...requiredPermissions
}: AuthProps): React.ReactElement => {
  const { hasCanViewRequestsCommandsPermission, hasCanApproveExecutePermission, isLoading } =
    useAuthorization();

  let countOfAppPermissions = Object.keys(AppPermissions).length; // used for developer error checking

  const disabled = <></>;

  // check if users have required permissions, if not, return disabled
  if (isLoading) return disabled;

  if (requiredPermissions.canViewRequestsCommands && !hasCanViewRequestsCommandsPermission) return disabled;
  countOfAppPermissions--;

  if (requiredPermissions.canApproveExecute && !hasCanApproveExecutePermission) return disabled;
  countOfAppPermissions--;

  // check if developer has accounted for all app permissions
  if (countOfAppPermissions !== 0)
    // please add condition to check if user has permission, if not return disabled
    throw new Error(
      "Developer Error: You have not accounted for all permissions in VisibleIf component"
    );

  // all requiredPermissions has been met
  return <>{children}</>;
};

export function AccessRestricted() {
  return <H1Text>Access Restricted</H1Text>;
}

const H1Text = styled("div")({
  font: "400 18px/24px Poppins",
  letterSpacing: "0em",
  textAlign: "left",
  color: "#C6C6C6",
});
