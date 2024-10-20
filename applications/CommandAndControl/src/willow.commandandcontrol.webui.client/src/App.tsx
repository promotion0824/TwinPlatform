import NotLoggedInPage from "./pages/NotLoggedIn";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import RemoteFetchProvider from "./providers/RemoteFetchProvider";
import { CssBaseline } from "@mui/material";
import { AppContextProvider } from "./providers/AppContextProvider.tsx";
import SnackbarProvider from "./providers/SnackbarProvider/SnackbarProvider.tsx";
import Layout from "./components/Layout.tsx";
import { RequestsCountContextProvider } from "./providers/RequestsCountProvider.tsx";

const App = () => {
  return (
    <RemoteFetchProvider>
      <CssBaseline />
      <AuthenticatedTemplate>
        <SnackbarProvider>
          <AppContextProvider>
            <RequestsCountContextProvider>
              <Layout />
            </RequestsCountContextProvider>
          </AppContextProvider>
        </SnackbarProvider>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <NotLoggedInPage />
      </UnauthenticatedTemplate>
    </RemoteFetchProvider>
  );
};

export default App;
