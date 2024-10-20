/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 * For more details on using MSAL.js with Azure AD B2C, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/working-with-b2c.md
 */

// See also https://dev.azure.com/willowdev/AzurePlatform/_git/aad-b2c
import env from './services/EnvService';
const b2cscopes = env.b2cscopes();

export const msalConfig = {
  auth: {
    clientId: env.clientId() ?? "Backend failed to set client id, check env.js",
    authority: env.authority() ?? "Backend failed to set authority, check env.js",
    knownAuthorities: env.knownAuthorities() ?? ["Backend failed to set known authorities, check env.js"],
    redirectUri: env.redirect() ?? "Backend failed to set redirect address, check env.js"
  },
  cache: {
    cacheLocation: "localStorage", // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
    //cacheLocation: "sessionStorage", // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
    storeAuthStateInCookie: false, // If you wish to store cache items in cookies as well as browser cache, set this to "true".
  }
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will [allegedly] add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest = {
    scopes: ["openid", "profile", "email", ...b2cscopes],
};

