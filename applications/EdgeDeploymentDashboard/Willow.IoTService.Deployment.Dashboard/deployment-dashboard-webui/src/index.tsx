import { ThemeProvider as WillowThemeProvider } from "@willowinc/ui";
import { ThemeProvider as MUIThemeProvider } from "@mui/material";
import theme from "./muiTheme";
import { PublicClientApplication } from '@azure/msal-browser';
import { MsalProvider } from '@azure/msal-react';
import { LicenseInfo } from '@mui/x-license-pro';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { initConfig, msalConfig } from './config'
import App from './components/App';
import ApplicationTypes from './components/ApplicationTypes';
import Deployer from './components/Deployer';
import Deployments from './components/Deployments';
import './index.css';
import reportWebVitals from './reportWebVitals';


LicenseInfo.setLicenseKey(
  "a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI="
);


const render = () => {
  const msalInstance = new PublicClientApplication(msalConfig);

  const root = ReactDOM.createRoot(
    document.getElementById('root') as HTMLElement
  );

  root.render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <WillowThemeProvider>
          <MUIThemeProvider theme={theme}>
            <BrowserRouter>
              <Routes>
                <Route path="*" element={<App />}>
                  <Route path="deployments" element={<Deployments setOpenError={() => null} />} />
                  <Route path="deployer" element={<Deployer setOpenError={() => null} />} />
                  <Route path="types" element={<ApplicationTypes setOpenError={() => null} />} />
                </ Route>
              </Routes>
            </BrowserRouter>
          </MUIThemeProvider>
        </WillowThemeProvider>
      </MsalProvider>
    </React.StrictMode>
  );

  // If you want to start measuring performance in your app, pass a function
  // to log results (for example: reportWebVitals(console.log))
  // or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
  reportWebVitals();
}

(function () {
  initConfig().then(() => render());
})();
