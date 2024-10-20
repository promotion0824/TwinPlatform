import { WindowWithEnv } from '../WindowWithEnv';

const env = (window as any as WindowWithEnv)._env_;

const EnvService = {
  baseurl: function () {
    return env.baseurl ?? "/";
  },

  baseapi: function () {
    return env.baseapi ?? "/";
  },

  customerName: function () {
    return env.customer;
  },

  customerId: function () {
    return env.customerId;
  },

  b2cscopes: function () {
    return env.b2cscopes ?? [];
  },

  clientId: function () {
    return env.clientId;
  },

  authority: function () {
    return env.authority;
  },

  knownAuthorities: function () {
    return env.knownAuthorities;
  },

  redirect: function () {
    //if redirect is relative path, use the current location
    if (env.redirect.startsWith('/')) {
      return (new URL(env.redirect, window.location.origin)).href;
    }
    return env.redirect;
  },

  appInsightsConnectionString: function () {
    return env.appInsightsConnectionString;
  }
};

export default EnvService;
