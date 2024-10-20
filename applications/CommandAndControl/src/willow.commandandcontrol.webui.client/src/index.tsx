import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { ThemeProvider as WillowThemeProvider } from "@willowinc/ui";
import { ThemeProvider as MUIThemeProvider } from "@mui/material";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig, apiConfig } from "./authConfig.ts";
import { LicenseInfo } from "@mui/x-license-pro";
import { configService } from "./services/ConfigService";
import theme from "./muiTheme";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import "default-passive-events";
import { routes } from "./routes.tsx";
import { ErrorFallback } from "./components/error/errorBoundary";

// MUI Pro License
LicenseInfo.setLicenseKey(
  "a914bcf589677e9d1a41977085571d48Tz04NjI2MixFPTE3NDE4NDc4MjIwMDAsUz1wcm8sTE09c3Vic2NyaXB0aW9uLEtWPTI="
);

const getBaseName = () => {
  const baseName = "/activecontrol";
  if (
    window.location.origin.includes("localhost") ||
    !window.location.pathname.includes(baseName)
  )
    return "/";

  return baseName;
};

const init = () => {
  configService.init().then((config) => {
    msalConfig.auth.redirectUri = new URL(
      getBaseName(),
      window.location.origin
    ).href;
    msalConfig.auth.clientId =
      config?.azureAppOptions?.clientId ?? "Backend failed to set client id";
    msalConfig.auth.authority =
      config?.azureAppOptions?.authority ?? "Backend failed to set authority";
    msalConfig.auth.knownAuthorities =
      config?.azureAppOptions?.knownAuthorities ??
      "Backend failed to set known authorities";
    apiConfig.b2cScopes =
      config?.azureAppOptions?.b2CScopes ?? "Backend failed to set b2cScopes";
    render();
  });
};

const router = createBrowserRouter(
  [
    {
      element: <App />,
      errorElement: <ErrorFallback />,
      children: [...routes],
    },
  ],
  {
    basename: getBaseName(),
  }
);

const render = () => {
  const msalInstance = new PublicClientApplication(msalConfig);
  ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <WillowThemeProvider>
          <MUIThemeProvider theme={theme}>
            <RouterProvider router={router} />
          </MUIThemeProvider>
        </WillowThemeProvider>
      </MsalProvider>
    </React.StrictMode>
  );
};

init();
