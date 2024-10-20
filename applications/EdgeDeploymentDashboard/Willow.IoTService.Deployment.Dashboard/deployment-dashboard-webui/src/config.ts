import axios from "axios";

const extensionUri = "edge-deployment-dashboard";

const baseUrl = (extensionName: string) => {
  if (
    window.location.origin.includes("localhost") ||
    !window.location.pathname.includes(extensionName)
  ) {
    return window.location.origin;
  }

  const urlArray = window.location.href.split(extensionName);
  return urlArray[0] + extensionName;
};

const getExtensionName = () => {
  const extensionName = "edge-deployment-dashboard";
  if (
    window.location.origin.includes("localhost") ||
    !window.location.pathname.includes(extensionName)
  )
    return "/";

  return extensionName;
};

export const endpoint =
  process.env.NODE_ENV === "development"
    ? "https://localhost:5000"
    : baseUrl(extensionUri);

var authConfig = {
  clientId: '',
  authority: 'https://localhost',
  redirectUri: new URL(getExtensionName(), window.location.origin).href,
  knownAuthorities: []
}

var authScopes: string[] = [];

async function getConfig() {
  const configRes = await axios({
    url: `${endpoint}/api/v1/config`,
    method: 'GET'
  });

  return configRes.data;
}

export const initConfig = async function () {
  const data = await getConfig();
  authConfig.clientId = data.azureAppOptions.clientId;
  authConfig.authority = data.azureAppOptions.authority;
  authConfig.knownAuthorities = data.azureAppOptions.knownAuthorities;

  authScopes = data.azureAppOptions.b2CScopes
  loginRequest.scopes = authScopes;
};

export const msalConfig = {
  auth: authConfig,
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  }
};

export const loginRequest = {
  scopes: authScopes
};
