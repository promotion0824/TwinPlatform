import { BrowserRouter as Router } from 'react-router-dom';
import NotLoggedInPage from './pages/NotLoggedIn';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import RequestInterceptor from './providers/RequestInterceptor';
import RemoteFetchProvider from './providers/RemoteFetchProvider';
import AppCustomer from './AppCustomer';
import env from './services/EnvService';
import { CssBaseline } from '@mui/material';
import { withAITracking } from "@microsoft/applicationinsights-react-js";
import { reactPlugin } from './components/appInsights';

export default withAITracking(reactPlugin, () => {

  const baseurl = env.baseurl();

  return (
    <RemoteFetchProvider>
      <CssBaseline />
      <Router basename={baseurl}>
        <AuthenticatedTemplate>
          <RequestInterceptor>
            <AppCustomer />
          </RequestInterceptor>
        </AuthenticatedTemplate>
        <UnauthenticatedTemplate>
          <NotLoggedInPage />
        </UnauthenticatedTemplate>
      </Router>
    </RemoteFetchProvider>
  );
});
