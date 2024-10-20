import { useQuery } from 'react-query';
import { AuthenticatedUserAndPolicyDecisionsDto, DecisionDto } from '../../Rules';
import { Tooltip } from '@mui/material';
import useApi from '../../hooks/useApi';
import { RouterNavLink } from '../nav/NavComponents';

interface Demands {
  canViewRules?: boolean | undefined;
  canViewInsights?: boolean | undefined;
  canViewCommands?: boolean | undefined;
  canViewAdminPage?: boolean | undefined;
  canViewSwitcher?: boolean | undefined;
  canCreateRules?: boolean | undefined;
  canEditRules?: boolean | undefined;
  canExportRules?: boolean | undefined;
  canDownloadRules?: boolean | undefined;
  // testing, always false
  canSummonGodzilla?: boolean | undefined;
}

interface AuthProps extends Demands {
  // And the content we want to protect
  children: React.ReactNode;
  // link if this is a link around children
  to?: any;
  //optional provide custom policy set
  policies?: AuthenticatedUserAndPolicyDecisionsDto;
}

interface AuthToProps extends AuthProps {
  // The link to protect
  to: any;
}

/**
 * Disables children unless user has permission
 * @param props
 */

export const ClickableIf = ({ children, to, ...demands }: AuthProps): React.ReactElement => {

  const api = useApi();

  const user = useQuery(['user'], async () => {
    const data = await api.getUserInfo('me');
    return data;
  });

  const disabled = (_demand: boolean, decision: DecisionDto) =>
    (<Tooltip title={decision.reason ?? "no reason"} enterDelay={2000}><RouterNavLink to={'#'} aria-disabled={true}>{children}</RouterNavLink></Tooltip>);

  if (user.isLoading || !user.data?.policyDecisions) return (<RouterNavLink to={'#'} aria-disabled={true}>{children}</RouterNavLink>);

  let policies = user.data;

  if (demands.policies) {
     policies = demands.policies!;
  }

  const policyDecisions = policies.policyDecisions!;

  const canViewRules: DecisionDto = (policyDecisions["CanViewRules"]) ?? new DecisionDto();
  const canViewInsights: DecisionDto = (policyDecisions && policyDecisions["CanViewInsights"]) ?? new DecisionDto();
  const canViewCommands: DecisionDto = (policyDecisions && policyDecisions["CanViewCommands"]) ?? new DecisionDto();
  const canViewAdminPage: DecisionDto = (policyDecisions && policyDecisions["CanViewAdminPage"]) ?? new DecisionDto();
  const canViewSwitcher: DecisionDto = (policyDecisions && policyDecisions["CanViewSwitcher"]) ?? new DecisionDto();
  const canEditRules: DecisionDto = (policyDecisions["CanEditRules"]) ?? new DecisionDto();
  const canExportRules: DecisionDto = (policyDecisions["CanExportRules"]) ?? new DecisionDto();
  const canDownloadRules: DecisionDto = (policyDecisions["CanDownloadRules"]) ?? new DecisionDto();
  const canCreateRules: DecisionDto = (policyDecisions["CanCreateRules"]) ?? new DecisionDto();
  const canSummonGodzilla: DecisionDto = new DecisionDto({ reason: "Nobody can do this", success: false });  // for testing

  if (demands.canViewRules && !canViewRules.success) return disabled(demands.canViewRules, canViewRules);
  if (demands.canViewInsights && !canViewInsights.success) return disabled(demands.canViewInsights, canViewInsights);
  if (demands.canViewCommands && !canViewCommands.success) return disabled(demands.canViewCommands, canViewCommands);
  if (demands.canViewAdminPage && !canViewAdminPage.success) return disabled(demands.canViewAdminPage, canViewAdminPage);
  if (demands.canViewSwitcher && !canViewSwitcher.success) return disabled(demands.canViewSwitcher, canViewSwitcher);
  if (demands.canEditRules && !canEditRules.success) return disabled(demands.canEditRules, canEditRules);
  if (demands.canExportRules && !canExportRules.success) return disabled(demands.canExportRules, canExportRules);
  if (demands.canDownloadRules && !canDownloadRules.success) return disabled(demands.canDownloadRules, canDownloadRules);
  if (demands.canCreateRules && !canCreateRules.success) return disabled(demands.canCreateRules, canCreateRules);
  if (demands.canSummonGodzilla && !canSummonGodzilla.success) return disabled(demands.canSummonGodzilla, canSummonGodzilla);

  // Otherwise OK, all demands have been met
  if (!to) return <>{children}</>;  // not a link

  return (<RouterNavLink to={to}>{children}</RouterNavLink>);
}

/**
 * Hides children unless user can view rules
 * @param props
 */

export const VisibleIf = ({ children, ...demands }: AuthProps): React.ReactElement => {

  const api = useApi();

  const user = useQuery(['user'], async () => {
    const data = await api.getUserInfo('me');
    return data;
  });

  const disabled = (_demand: boolean, _decision: DecisionDto) => <></>;

  if (user.isLoading || !user.data?.policyDecisions) return (<></>);

  let policies = user.data;

  if (demands.policies) {
    policies = demands.policies!;
  }

  const policyDecisions = policies.policyDecisions!;

  const canViewRules: DecisionDto = (policyDecisions["CanViewRules"]) ?? new DecisionDto();
  const canViewInsights: DecisionDto = (policyDecisions && policyDecisions["CanViewInsights"]) ?? new DecisionDto();
  const canViewCommands: DecisionDto = (policyDecisions && policyDecisions["CanViewCommands"]) ?? new DecisionDto();
  const canViewAdminPage: DecisionDto = (policyDecisions && policyDecisions["CanViewAdminPage"]) ?? new DecisionDto();
  const canViewSwitcher: DecisionDto = (policyDecisions && policyDecisions["CanViewSwitcher"]) ?? new DecisionDto();
  const canEditRules: DecisionDto = (policyDecisions["CanEditRules"]) ?? new DecisionDto();
  const canExportRules: DecisionDto = (policyDecisions["CanExportRules"]) ?? new DecisionDto();
  const canDownloadRules: DecisionDto = (policyDecisions["CanDownloadRules"]) ?? new DecisionDto();
  const canCreateRules: DecisionDto = (policyDecisions["CanCreateRules"]) ?? new DecisionDto();
  const canSummonGodzilla: DecisionDto = new DecisionDto({ reason: "Nobody can do this", success: false });  // for testing

  if (demands.canViewRules && !canViewRules.success) return disabled(demands.canViewRules, canViewRules);
  if (demands.canViewInsights && !canViewInsights.success) return disabled(demands.canViewInsights, canViewInsights);
  if (demands.canViewCommands && !canViewCommands.success) return disabled(demands.canViewCommands, canViewCommands);
  if (demands.canViewAdminPage && !canViewAdminPage.success) return disabled(demands.canViewAdminPage, canViewAdminPage);
  if (demands.canViewSwitcher && !canViewSwitcher.success) return disabled(demands.canViewSwitcher, canViewSwitcher);
  if (demands.canEditRules && !canEditRules.success) return disabled(demands.canEditRules, canEditRules);
  if (demands.canExportRules && !canExportRules.success) return disabled(demands.canExportRules, canExportRules);
  if (demands.canDownloadRules && !canDownloadRules.success) return disabled(demands.canDownloadRules, canDownloadRules);
  if (demands.canCreateRules && !canCreateRules.success) return disabled(demands.canCreateRules, canCreateRules);
  if (demands.canSummonGodzilla && !canSummonGodzilla.success) return disabled(demands.canSummonGodzilla, canSummonGodzilla);

  // Otherwise OK, all demands have been met
  return <>{children}</>;
}

export const NavCan = ({ to, children, ...demands }: AuthToProps) => {
  return (<ClickableIf {...demands} to={to}>{children}</ClickableIf>);
}
