import axios from 'axios'
import { UserManager, OidcClient } from 'oidc-client-ts'
import getUrl, { getMobileUrl } from './api'
import localStorage from './localStorage'
import qs from './qs'

/**
 * What happens when a user visits a page and they're not logged in?
 *
 * Suppose the user lands at http://command.willowinc.com/tickets/9d1fa7f3-3b3d-4c52-add9-3237199ed96d
 *
 * 1. Either the `AppContent` or `Authenticated` components will redirect to
 *    `authService.getLoginPath()`. This path is just "/account/login" with
 *    the original path put in the query variable `originalPath`.
 * 2. The `/account/login` route renders the `Login` component, which calls
 *    `authService.login`.
 * 3. This redirects to B2C, but first stores the `originalPath` in the sign in state.
 * 4. On successful login, B2C redirects to `/account/authcallback`
 * 5. The `/account/authcallback` route renders the `AuthCallback` component which
 *    calls `authService.readSigninResponse`. This function makes a request to PortalXL
 *    for sign in, and then redirects back to the original path from step 1.
 */

const SIGN_IN_TYPES = {
  signIn: 0,
  resetPassword: 1,
  silentRenew: 2,
}

const minutes = 60 * 1000
const SILENT_RENEW_TRIGGER_BUFFER = 30 * minutes

const buildUserManager = (config, policyId = null, redirectUrl = null) => {
  const localPolicyId = policyId ?? config.AzureAdB2C.SignUpSignInPolicyId
  const localRedirectUrl =
    redirectUrl ?? `${window.location.origin}/account/authcallback`

  const settings = {
    authority: `${config.AzureAdB2C.Instance}/${config.AzureAdB2C.Domain}/${localPolicyId}/v2.0/`,
    client_id: config.AzureAdB2C.ClientId,
    redirect_uri: localRedirectUrl,
    response_type: 'code',
    scope: config.AzureAdB2C.ClientId,
    automaticSilentRenew: false,
    validateSubOnSilentRenew: false,
    monitorSession: true,
    monitorAnonymousSession: false,
    filterProtocolClaims: true,
    loadUserInfo: true,
    revokeTokensOnSignout: true,
    silent_redirect_uri: `${window.location.origin}${
      config.AzureAdB2C.SilentRenewCallbackPath ??
      '/account/silentrenewcallback'
    }`,
    includeIdTokenInSilentRenew: false,
    post_logout_redirect_uri: `${window.location.origin}${
      config.AzureAdB2C.PostLogoutRedirectPath ?? '/account/login'
    }`,
    ui_locales: config.language,
  }

  return new UserManager(settings)
}

const redirectToUserflow = async (
  config,
  policyId,

  /**
   * This is where we tell B2C to redirect to after login. This is a fixed value per environment
   * and must exactly match an entry in the B2C application.
   */
  callbackUrl,

  /**
   * This is where we ultimately want to get back to. We will redirect here after the B2C redirect is finished.
   */
  originalPath
) => {
  const userManager = buildUserManager(config, policyId, callbackUrl)

  await userManager.signinRedirect({
    redirect_uri: callbackUrl,
    state: {
      redirectUrl: originalPath,
    },
  })
}

const startSilentRenewFlow = async (config) => {
  const userManager = buildUserManager(config)

  try {
    await userManager.signinSilent({
      response_mode: 'query',
    })
  } catch (err) {
    if (err.message !== 'Frame window timed out') throw err
  }
}

export const getAuthConfigKey = (userId) => `wr-auth-${userId}`

export const setAuthConfigToLocalStorage = (userId, expiresInSeconds) => {
  if (userId) {
    localStorage.set(getAuthConfigKey(userId), {
      expiresAt: Date.now() + expiresInSeconds * 1000,
    })
  }
}

export const getAuthConfigFromLocalStorage = (userId) =>
  localStorage.get(getAuthConfigKey(userId))

/**
 * Call this when we are at the auth callback endpoint. The path will be something like
 * https://.../account/authcallback?state=...&code=...
 * This function takes the `state` and `code` parameters from the path, parses
 * the state, does some checks, and if everything is good, it uses the data to
 * sign into PortalXL via the /api/me/signin endpoint.
 *
 * `app` should be "platform" or "mobile" and is used to determine which sign
 * in endpoint is used.
 */
const readCallbackResponse = async (signInType, app) => {
  const client = new OidcClient({ response_mode: 'query' })

  const { state, response } = await client.readSigninResponseState(
    window.location.href
  )

  // Note: as distinct from getUrl which is for API URLs and which will
  // include a region like /au.
  function getPath(basePath) {
    if (app === 'mobile') {
      return '/mobile-web' + basePath
    } else {
      return basePath
    }
  }

  if (state.request_type === 'si:r' && response.error_description) {
    // User navigates to reset password
    if (response.error_description.indexOf('AADB2C90118') > -1) {
      window.location.href = getPath('/account/forgot-password')
      return
    }

    // User cancels reset password operation
    if (response.error_description.indexOf('AADB2C90091') > -1) {
      window.location.href = getPath('/account/login')
      return
    }
  }

  const isSilentRenew = signInType === SIGN_IN_TYPES.silentRenew
  const redirectUrl = `${window.location.origin}/account/${
    isSilentRenew ? 'silentrenewcallback' : 'authcallback'
  }`

  const signInRequest = {
    AuthorizationCode: response.code,
    CodeVerifier: state.code_verifier,
    SignInType: signInType,
    RedirectUri: redirectUrl,
  }

  try {
    const { userId, expiresIn } = (
      await axios.post(
        app === 'mobile'
          ? getMobileUrl('/api/signin')
          : getUrl('/api/me/signin'),
        signInRequest
      )
    ).data

    setAuthConfigToLocalStorage(userId, expiresIn)

    if (!isSilentRenew) {
      window.location.href = state.data?.redirectUrl ?? getPath('/')
    }
  } catch (err) {
    if (!isSilentRenew && err.response.status === 403) {
      window.location.href = getPath('/account/no-permissions')
    } else {
      throw err
    }
  }
}

const authService = {
  login: async (config) => {
    await redirectToUserflow(
      config,
      config.AzureAdB2C.SignUpSignInPolicyId,
      `${window.location.origin}${
        config.AzureAdB2C.RedirectPath ?? '/account/authcallback'
      }`,
      qs.get('originalPath')
    )
  },

  /*
   * Sign in, assuming we are at a URL that looks like https://.../account/authcallback?state=...&code=...
   * `app` should be "platform" or "mobile" and is used to determine which sign
   * in endpoint is used.
   */
  readSigninResponse: async (app) => {
    await readCallbackResponse(SIGN_IN_TYPES.signIn, app)
  },

  silentRenew: async (config) => {
    await startSilentRenewFlow(config)
  },

  /*
   * Silent renew, assuming we are at a URL that looks like https://.../account/authcallback?state=...&code=...
   * `app` should be "platform" or "mobile" and is used to determine which sign
   * in endpoint is used.
   */
  readSilentRenewResponse: async (app) => {
    await readCallbackResponse(SIGN_IN_TYPES.silentRenew, app)
  },
  logout: async (config) => {
    const userManager = buildUserManager(config)

    await userManager.signoutRedirect()
  },

  signoutRedirectCallback: async () => {
    const userManager = new UserManager({})
    userManager.clearStaleState()
  },

  resetPassword: async (config) => {
    await redirectToUserflow(
      config,
      config.AzureAdB2C.PasswordResetPolicyId,
      `${window.location.origin}/account/resetpasswordcallback`
    )
  },

  readResetPasswordResponse: async (app) => {
    await readCallbackResponse(SIGN_IN_TYPES.resetPassword, app)
  },

  checkAuthExpirationWithSilentRenewal: async (userId, config) => {
    if (userId) {
      const expiresAt = getAuthConfigFromLocalStorage(userId)?.expiresAt
      if (!expiresAt || expiresAt - Date.now() < SILENT_RENEW_TRIGGER_BUFFER) {
        await startSilentRenewFlow(config)
      }
    }
  },

  /**
   * Go to this path to bring up the login page. The path will include
   * a parameter that will be used to redirect back to the current location
   * on successful login.
   */
  getLoginPath: () => {
    const redirectTo =
      window.location.pathname !== '/account/login'
        ? window.location.pathname /* possible to be '/' */ +
          window.location.search +
          window.location.hash
        : '/'

    return `/account/login${
      redirectTo !== '/'
        ? `?originalPath=${encodeURIComponent(redirectTo)}`
        : ''
    }`
  },
}

export default authService
