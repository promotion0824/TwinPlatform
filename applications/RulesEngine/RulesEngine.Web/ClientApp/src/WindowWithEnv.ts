export interface WindowWithEnv {
  _env_: {
    /**
     * Customer name
     */
    customer: string;
    /**
     * Customer Id
     */
    customerId: string;
    /**
     * Redirect for login
     */
    redirect: string;
    /**
     * Base Url for react app and all other static asset requests
     * Necessary because the app runs on a path not at a domain root URL
     */
    baseurl: string;
    /**
     * Base Api for API calls, could be on a different authority for locally hosted CORS-enabled sites
     */
    baseapi: string;
    /**
     * B2C client id
     */
    clientId: string;
    /**
     * B2C Authority
     */
    authority: string;
    /**
     * B2C known authorities
     */
    knownAuthorities: string[];
    /**
     * B2C scopes to request for calling backend APIs
     */
    b2cscopes: string[];
    /**
     * Version of app that's running
     */
    version: string;
    /**
     * App insights key (deprecated)
     */
    appInsightsKey: string;
    /**
     * App insights connection string
     */
    appInsightsConnectionString: string
  };
}
