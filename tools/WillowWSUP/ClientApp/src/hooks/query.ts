import { useQuery } from "react-query";
import { AdminAppService, OpenAPI } from "../generated";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { SilentRequest } from "@azure/msal-browser";

const query = () => {

  const { instance, accounts } = useMsal();

  const logout = () => {
    const logoutRequest = {
      account: accounts[0]
    }
    instance.logoutRedirect(logoutRequest);
  };

  const token = useQuery(['token', accounts], async (_x) => {
    const account = accounts[0];
    const silentRequest: SilentRequest = { account: account, scopes: loginRequest.scopes };
    try {
      const res = await instance.acquireTokenSilent(silentRequest);
      return res;
    }
    catch (e) {
      console.log('Failed to obtain token', e);
      logout();
    }
    return null;
  });

  const { data, isFetched } = useQuery(['getstate'], async () => {

    OpenAPI.TOKEN = token.data!.accessToken;
    OpenAPI.WITH_CREDENTIALS = true;

    const data = await AdminAppService.state();

    return data;
  }, { keepPreviousData: true, refetchInterval: 20000, enabled: !!token.data });

  return { data, isFetched };
};

export default query;
