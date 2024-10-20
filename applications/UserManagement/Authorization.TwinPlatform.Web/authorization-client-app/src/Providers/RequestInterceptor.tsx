import { IPublicClientApplication } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import axios, { AxiosInstance } from "axios";
import { loginRequest } from "../authConfig";
import { getAuthClient } from "../axiosClients";
import { endpoints } from "../Config";
import { IAccountInfo, useAccountInfo } from "../Hooks/useAccountInfo";

let requestInterceptorId: number | null = null;
let responseInterceptorId: number | null = null;

export default function RequestInterceptor({ children }: { children: React.ReactNode }): JSX.Element {

  const { instance } = useMsal();
  const accountInfo = useAccountInfo();
  const axiosClient = getAuthClient();

  if (requestInterceptorId === null) {
    requestInterceptorId = InterceptRequest(axiosClient, accountInfo, instance);
    console.log('Registering Request Interceptor')
  }

  if (responseInterceptorId === null) {
    responseInterceptorId = InterceptResponse(axiosClient);
    console.log('Registering Response Interceptor')
  }

  return (<>{children}</>);
}


const InterceptRequest = (axiosClient: AxiosInstance, accountInfo: IAccountInfo | null, msalInstance: IPublicClientApplication): number => {

  let reqId = axiosClient.interceptors.request.use(
    async function (config) {
      // Logic here before the request is sent
      if (accountInfo?.account && config.headers) {

        try {
          const response = await msalInstance.acquireTokenSilent({
            ...loginRequest(), account: accountInfo.account
          });
          const bearer = `Bearer ${response.accessToken}`;
          config.headers.Authorization = bearer;
        }
        catch (e) {
          console.log(e);
          await msalInstance.loginRedirect();
        }

      } else {
        // if the accountInfo is null - user might have not logged in

        try {

          await msalInstance.loginRedirect();
        } catch (e) {
          console.log(e);
        }

      }
      return config;
    },
    function (error) {
      // Logic for error
      console.error(error.response);
      return Promise.reject(error);
    }
  );

  return reqId;
};


const InterceptResponse = (axiosClient: AxiosInstance): number => {

  let resId = axios.interceptors.response.use(
    function (response) {
      return response;
    },
    function (error) {
      return Promise.reject(error);
    }
  );
  return resId;
};
