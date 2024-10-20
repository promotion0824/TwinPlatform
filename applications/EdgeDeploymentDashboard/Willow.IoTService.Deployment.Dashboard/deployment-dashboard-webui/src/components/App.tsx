import { InteractionStatus } from '@azure/msal-browser';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from '@azure/msal-react';
import { loginRequest } from '../config';
import Layout from './Layout';

function App() {
  const isAuthenticated = useIsAuthenticated();
  const { instance, inProgress } = useMsal();

  if (inProgress === InteractionStatus.None && !isAuthenticated) {
    instance.loginRedirect(loginRequest);
  }

  return (
    <>
      <AuthenticatedTemplate>
        <Layout />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <div>Redirecting...</div>
      </UnauthenticatedTemplate>
    </>
  );
}

export default App;
