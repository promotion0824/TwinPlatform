import * as React from 'react';
import { AxiosInstance } from 'axios';
import { useMsal, useAccount } from '@azure/msal-react';
import { loginRequest } from '../authConfig';
import { GetTlmClient } from '../axiosWithToken';
import { useRef } from 'react';

const correlationHeaderName = 'x-correlation-id';

export const AxiosInterceptor = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0]);
  const client = GetTlmClient();
  const shouldRegisterInterceptor = useRef(true);

  if (shouldRegisterInterceptor.current) {
    registerRequestIntercepting(client, account, instance);
    registerResponseIntercepting(client);
    shouldRegisterInterceptor.current = false;
  }
  return <>{children}</>;
};

const registerRequestIntercepting = async (axiosClient: AxiosInstance, account: any, forInstance: any) => {
  axiosClient.interceptors.request.use(
    async (config: any) => {
      if (account && config.headers) {
        try {
          const response = await forInstance.acquireTokenSilent({
            ...loginRequest(),
            account,
          });

          const bearer = `Bearer ${response.accessToken}`;
          config.headers.Authorization = bearer;
          localStorage.setItem('token', bearer);
        } catch (e) {
          // InteractionRequiredAuthError: interaction_required: AADB2C90077: User does not have an existing session and request prompt parameter has a value of 'None'.
          console.error(e);
          localStorage.setItem('token', '');
          await forInstance.loginRedirect();
        }
      } else {
        try {
          await forInstance.loginRedirect();
        } catch (e) {
          console.error(e);
        }
      }

      return config;
    },
    (error: any) => {
      if (401 === error.response?.status || 403 === error.response?.status) {
        localStorage.setItem('token', '');
        window.location.reload();
      }
      return Promise.reject(error);
    }
  );
};

const registerResponseIntercepting = async (axiosClient: AxiosInstance) => {
  axiosClient.interceptors.response.use(
    async (response: any) => {
      // Any status code that lie within the range of 2xx cause this function to triggers
      return response;
    },
    async (error: any) => {
      return getErrorMessage(error);
    }
  );
};

const getErrorMessage = async (error: any) => {
  if ((!error || !error?.message) && error.response === undefined) {
    return Promise.reject('The server failed to respond.');
  }

  let responseMessage;
  let correlationIdMessage = '';
  const currentPage = `\nCurrent Page: ${window.location.href}`;

  if (typeof error.response !== 'undefined') {
    if (error.response.data instanceof Blob) {
      await error.response.data
        .text()
        .then((value: any) => {
          responseMessage = JSON.parse(value).message;
        })
        .catch(() => { });
    } else {
      if (typeof error.response.data !== 'undefined') {
        responseMessage = error.response.data.message;
      }
    }

    //check if correlation Id is present
    if (error.response.headers[correlationHeaderName] !== undefined) {
      let correlationId = error.response.headers[correlationHeaderName];
      correlationIdMessage = `\nCorrelation Id: ${correlationId}`;
    }
  }

  let statusCode = error.response?.status;
  let message;

  if (responseMessage && responseMessage !== '') {
    message = responseMessage;
  } else {
    switch (statusCode) {
      case 400:
        message = "The request you've sent is invalid";
        break;
      case 401:
        message = 'Failed to authenticate. Please refresh and try again.';
        break;
      case 403:
        message = 'You are not authorized to do this action.';
        break;
      case 404:
        message = "Action you're trying to execute was not found.";
        break;
      case 422:
        message = 'Your request failed due to the incorrect or missing data.';
        break;
      case 424:
        message = 'Your request failed due to the exception happen on the server side.';
        break;
      default:
        message = error?.message;
    }
  }

  return Promise.reject(`Status code: ${statusCode ?? 'not available'}\nMessage: ${message}${correlationIdMessage}${currentPage}`);
};
