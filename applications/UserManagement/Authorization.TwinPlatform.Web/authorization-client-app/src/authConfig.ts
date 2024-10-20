import { ConfigModel } from "./types/ConfigModel";

export const msalConfig = {
  auth: {
    clientId: "Comes-From-Config-Endpoint",
    authority: "Comes-From-Config-Endpoint",
    knownAuthorities: ["Comes-From-Config-Endpoint"],
    redirectUri: "Comes-From-Config-Endpoint",
  },
  cache: {
    cacheLocation: "localStorage", // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  }
};

export const tokenConfig = {
  apiB2CScopes: ['Comes-From-Config-Endpoint'],
};

// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest = () => {
  return { scopes: ['openid', 'profile', 'email', ...tokenConfig.apiB2CScopes] }
};

export const UpdateAuthConfig = (configModel: ConfigModel) => {

  tokenConfig.apiB2CScopes = configModel._spaConfig.apiB2CScopes;
  msalConfig.auth.clientId = configModel._spaConfig.clientId;
  msalConfig.auth.authority = configModel._spaConfig.authority;
  msalConfig.auth.knownAuthorities = configModel._spaConfig.knownAuthorities;
  msalConfig.auth.redirectUri = new URL(configModel._spaConfig.baseName, window.location.origin).href;
}
