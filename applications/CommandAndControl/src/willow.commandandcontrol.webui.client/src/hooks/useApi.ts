import { Client } from "../services/Clients";
import { endpoint } from "../config";
import { useMsal, useAccount } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { getAxiosClient } from "../services/AxiosClient";

let clientInstance: Client | null = null;

const useApi = () => {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0]);
  const axiosClient = getAxiosClient();

  if (!clientInstance) {
    axiosClient.interceptors.request.use(
      async (config) => {
        if (account) {
          try {
            let request = loginRequest();
            const response = await instance.acquireTokenSilent({
              ...request,
              account,
            });
            const bearer = `Bearer ${response.accessToken}`;
            config.headers!.Authorization = bearer;
            localStorage.setItem("token", bearer);
          } catch (e) {
            // InteractionRequiredAuthError: interaction_required: AADB2C90077: User does not have an existing session and request prompt parameter has a value of 'None'.
            console.error("AcquireTokenSilent failed", e);
            localStorage.setItem("token", "");
            const logoutRequest = {
              account: accounts[0],
            };
            await instance.logoutRedirect(logoutRequest);
            throw e;
          }
        } else {
          try {
            await instance.loginRedirect();
          } catch (e) {
            console.error(e);
          }
        }
        return config;
      },
      (error) => {
        if (401 === error.response.status) {
          localStorage.setItem("token", "");
          window.location.reload();
          return Promise.reject(error);
        } else {
          return Promise.reject(error);
        }
      }
    );

    clientInstance = new Client(endpoint, axiosClient);
  }

  return clientInstance;
};

export default useApi;
