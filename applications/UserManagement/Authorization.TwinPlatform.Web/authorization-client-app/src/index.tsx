
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import './index.css';
import reportWebVitals from './reportWebVitals';
import { PublicClientApplication } from "@azure/msal-browser";
import {
  MsalProvider,
  AuthenticatedTemplate,
  UnauthenticatedTemplate
} from "@azure/msal-react";
import { msalConfig, UpdateAuthConfig } from "./authConfig";
import AuthDarkTheme from './AuthTheme';
import { ThemeProvider as WillowThemeProvider } from "@willowinc/ui";
import { CssBaseline, ThemeProvider as MUIThemeProvider } from '@mui/material';
import LoginPage from './Pages/LoginPage';
import { ConfigClient } from './Services/AuthClient';
import { InitializeAppInsights } from './AppInsightsLogger';
import { endpoints } from './Config';
import { UpdateWillowContext } from './willowAppContext';
import { LicenseInfo } from '@mui/x-license-pro';

// MUI Pro License
// Copied from Deployment Dashboard, find a better way to set the license info
LicenseInfo.setLicenseKey(
  "a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI="
);


function InitApp(baseName: string) {
  const root = ReactDOM.createRoot(
    document.getElementById('root') as HTMLElement
  );
  const msalInstance = new PublicClientApplication(msalConfig);

  root.render(
    <MsalProvider instance={msalInstance}>
      <WillowThemeProvider>
        <MUIThemeProvider theme={AuthDarkTheme}>
        <BrowserRouter basename={baseName}>
          <UnauthenticatedTemplate>
            <LoginPage />
          </UnauthenticatedTemplate>
          <AuthenticatedTemplate>
            <App />
          </AuthenticatedTemplate>
          </BrowserRouter>
        </MUIThemeProvider>
        </WillowThemeProvider>
    </MsalProvider>
  );
}

//Get Config to Initialize the App

ConfigClient.GetConfig().then((configModel) => {

  // baseName should endwith a forward slash
  if (!configModel._spaConfig.baseName.endsWith('/')) {
    configModel._spaConfig.baseName += '/';
  }
  // set endpoints baseName
  endpoints.baseName = configModel._spaConfig.baseName;

  UpdateAuthConfig(configModel);

  UpdateWillowContext(configModel._willowContext);

  InitializeAppInsights(configModel._appInsightSettings);

  InitApp(configModel._spaConfig.baseName);
});

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
