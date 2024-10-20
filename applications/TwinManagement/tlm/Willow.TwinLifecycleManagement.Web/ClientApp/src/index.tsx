import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { PublicClientApplication } from '@azure/msal-browser';
import { AuthenticatedTemplate, MsalProvider, UnauthenticatedTemplate } from '@azure/msal-react';
import { msalConfig, apiConfig } from './authConfig';
import { AxiosInterceptor } from './providers/AxiosInterceptor';
import { NotLoggedIn } from './pages/NotLoggedIn';
import TlmFetchProvider from './providers/FetchProvider';
import { configService } from './services/ConfigService';
import TLMTheme from './TLMTheme';
import { CssBaseline, ThemeProvider as MUIThemeProvider, StyledEngineProvider } from '@mui/material';
import { LicenseInfo } from '@mui/x-license-pro';
import { ThemeProvider as WillowThemeProvider } from '@willowinc/ui';
import SnackbarProvider from './providers/SnackbarProvider/SnackbarProvider';
import { HotKeys } from 'react-hotkeys';
import AppHotKeyMap from './HotKeyMap';
import styled from '@emotion/styled';
import { MantineProvider } from '@mantine/core';

// MUI Pro License
LicenseInfo.setLicenseKey(
  'a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI='
);

var baseurl = '/';

const init = () => {
  configService.init().then((config) => {
    document.title = 'TLM ' + config?.willowContext?.customerInstanceConfiguration?.customerInstanceName || '';
    baseurl = config.azureAppOptions.baseUrl ?? '/';
    msalConfig.auth.redirectUri = new URL(baseurl, window.location.origin).href;
    msalConfig.auth.clientId = config.azureAppOptions.clientId ?? 'Backend failed to set client id';
    msalConfig.auth.authority = config.azureAppOptions.authority ?? 'Backend failed to set authority';
    msalConfig.auth.knownAuthorities =
      config.azureAppOptions.knownAuthorities ?? 'Backend failed to set known authorities';
    apiConfig.b2cScopes = config.azureAppOptions.frontendB2CScopes ?? ['Backend failed to set frontendB2CScopes'];

    render();
  });
};

const render = () => {
  const msalInstance = new PublicClientApplication(msalConfig);
  const rootElement = document.getElementById('root');
  const root = createRoot(rootElement!);
  root.render(
    <TlmFetchProvider>
      <MsalProvider instance={msalInstance}>
        <BrowserRouter basename={baseurl}>
          <StyledEngineProvider injectFirst>
            <MUIThemeProvider theme={TLMTheme}>
              <MantineProvider>
                <WillowThemeProvider>
                  <CssBaseline />
                  <AuthenticatedTemplate>
                    <AxiosInterceptor>
                      <SnackbarProvider>
                        <StyledHotKeys keyMap={AppHotKeyMap}>
                          <App />
                        </StyledHotKeys>
                      </SnackbarProvider>
                    </AxiosInterceptor>
                  </AuthenticatedTemplate>
                  <UnauthenticatedTemplate>
                    <NotLoggedIn />
                  </UnauthenticatedTemplate>
                </WillowThemeProvider>
              </MantineProvider>
            </MUIThemeProvider>
          </StyledEngineProvider>
        </BrowserRouter>
      </MsalProvider>
    </TlmFetchProvider>
  );

  // If you want to start measuring performance in your app, pass a function
  // to log results (for example: reportWebVitals(console.log))
  // or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
  reportWebVitals();
};

init();

// Add 100% height to HotKeys div component, so children components are able to use 100% page layout height
// TODO: should we localize hotkey component to the specific pages that uses it. Right now, only copilot chat pages uses it.
const StyledHotKeys = styled(HotKeys)({ height: '100%' });
