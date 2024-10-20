/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 * For more details on using MSAL.js with Azure AD B2C, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/working-with-b2c.md
 */

// See also https://dev.azure.com/willowdev/AzurePlatform/_git/aad-b2c

export const msalConfig = {
  auth: {
    clientId: 'This is replaced dynamically per environment, see env.js',
    authority: 'This is replaced dynamically per environment, see env.js',
    knownAuthorities: ['This is replaced dynamically per environment, see env.js'],
    redirectUri: 'This is replaced dynamically per environment, see env.js',
  },
  cache: {
    cacheLocation: 'localStorage', // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
    storeAuthStateInCookie: false, // If you wish to store cache items in cookies as well as browser cache, set this to "true".
  },
};

// Scopes to request at login. Oddly if this is empty we don't even get the email back ??
export const apiConfig = {
  b2cScopes: ['This is replaced dynamically per environment, see env.js'],
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will [allegedly] add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest = function () {
    return {
        scopes: ['openid', 'profile', 'email', ...apiConfig.b2cScopes],
    };
};
