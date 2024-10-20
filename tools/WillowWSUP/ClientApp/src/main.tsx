import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import {
  ThemeProvider as MuiThemeProvider,
} from "@mui/material";
import getTheme from "@willowinc/mui-theme";
import {
  ThemeProvider as UiThemeProvider,
} from "@willowinc/ui";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import { msalConfig } from "./authConfig";
import "./index.css";
const msalInstance = new PublicClientApplication(msalConfig);

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <MuiThemeProvider theme={getTheme()}>
      <UiThemeProvider name="dark">
        <MsalProvider instance={msalInstance}>
          <App />
        </MsalProvider>
      </UiThemeProvider>
    </MuiThemeProvider>
  </React.StrictMode>
);
