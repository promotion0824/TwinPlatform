import { PublicClientApplication } from '@azure/msal-browser';
import { MsalProvider } from '@azure/msal-react';
import { msalConfig } from './authConfig';
import ReactDOM from 'react-dom/client';
import App from './App';

import mergedTheme from "./muiThemeOptions";
import { StyledEngineProvider, ThemeProvider as MuiThemeProvider } from '@mui/material';
import { ThemeProvider as WillowThemeProvider } from "@willowinc/ui";
import { LicenseInfo } from '@mui/x-license-pro';

// MUI Pro License
LicenseInfo.setLicenseKey(
  'a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI='
);

const msalInstance = new PublicClientApplication(msalConfig);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <StyledEngineProvider injectFirst> {/*This is needed until we are fully on the @willowinc/ui library*/}
    <WillowThemeProvider>
      <MuiThemeProvider theme={mergedTheme}>
        <MsalProvider instance={msalInstance}>
          <App />
        </MsalProvider>
      </MuiThemeProvider>
    </WillowThemeProvider>
  </StyledEngineProvider>
);
