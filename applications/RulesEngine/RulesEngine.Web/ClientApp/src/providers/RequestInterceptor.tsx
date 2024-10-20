import React from 'react';
import { useMsal, useAccount } from '@azure/msal-react';
import axios from 'axios';
import { loginRequest } from '../authConfig';
import { appInsights } from '../components/appInsights';

interface RequestInterceptorProps {
  children: JSX.Element,
}

const RequestInterceptor: React.FC<RequestInterceptorProps> = ({ children }: RequestInterceptorProps) => {

  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0]);
  /* eslint-disable no-param-reassign */

  axios.interceptors.request.use(async (config) => {

    if (account && !config.headers!.Authorization) {

      try {
        //comment/uncomment to test
        // console.log('throwing auth error manually')
        // throw new AuthError('401', 'Unauthorized')

        //comment/uncomment rest of try
        const response = await instance.acquireTokenSilent({
          ...loginRequest,
          account,
        });

        const bearer = `Bearer ${response.accessToken}`;
        config.headers!.Authorization = bearer;
      }
      catch (e: any) {
        // InteractionRequiredAuthError: interaction_required: AADB2C90077: User does not have an existing session and request prompt parameter has a value of 'None'.
        console.log('AcquireTokenSilent failed', e);
        //track failed auth
        const metricData = {
          average: 1,
          name: "Failed or expired login",
          sampleCount: 1
        };
        console.log('tracking auth');
        const additionalProperties = { "Request Interceptor": 'Auth Error' };
        // appInsights.loadAppInsights();
        appInsights.trackMetric(metricData, additionalProperties);

        const logoutRequest = {
          account: accounts[0]
        }
        instance.logoutRedirect(logoutRequest);
        throw (e);
        //const redirecturl = (window as any as WindowWithEnv)._env_.redirect ?? "/";
        //window.location.href = redirecturl;
      }
    }

    return config;
  }, (error: any) => {
    if (401 === error.response.status) {
      alert("Session expired");
      //config.headers!.Authorization = bearer;
      //window.location = '/login';
      return Promise.reject(error);
    } else {
      return Promise.reject(error);
    }
  });
  /* eslint-enable no-param-reassign */

  return (
    <>
      {children}
    </>
  );
};

export default RequestInterceptor;

// See https://medium.com/sopra-steria-norge/msal-react-automatically-sign-in-users-and-get-app-roles-30893449ed87
