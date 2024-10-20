import { useQuery } from "react-query";
import { DecisionDto } from "../Rules";
import useApi from "./useApi";

const useAuthorization = () => {

  const api = useApi();

  const user = useQuery(['user'], async (_x) => {
    const data = await api.getUserInfo('me');
    return data;
  });

  const canViewRules: DecisionDto = (user.data?.policyDecisions && user.data.policyDecisions["CanViewRules"]) ?? new DecisionDto();
  const canViewInsights: DecisionDto = (user.data?.policyDecisions && user.data.policyDecisions["CanViewInsights"]) ?? new DecisionDto();
  const canViewCommands: DecisionDto = (user.data?.policyDecisions && user.data.policyDecisions["CanViewCommands"]) ?? new DecisionDto();
  const canViewAdminPage: DecisionDto = (user.data?.policyDecisions && user.data.policyDecisions["CanViewAdminPage"]) ?? new DecisionDto();
  const canViewSwitcher: DecisionDto = (user.data?.policyDecisions && user.data.policyDecisions["CanViewSwitcher"]) ?? new DecisionDto();
  const defaultPermisson = new DecisionDto({ success: true, reason: 'Everyone can see this' });

  return {
    canViewRules,
    canViewInsights,
    canViewCommands,
    canViewAdminPage,
    canViewSwitcher,
    defaultPermisson
  };
}

export default useAuthorization;
