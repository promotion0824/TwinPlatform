import { useAppInsightsContext } from '@microsoft/applicationinsights-react-js'
import { authService, localStorage } from '@willow/common'
import { CustomerStatus } from '@willow/common/constants'
import { useApi } from '@willow/ui'
import _ from 'lodash'
import { useSnackbar } from 'providers/SnackbarProvider/SnackbarContext'
import { useEffect, useRef, useState } from 'react'
import { TemperatureUnit } from '../../../../common/src/types/types'
import { VIEW } from '../../constants/index'
import { useAnalytics } from '../AnalyticsProvider/AnalyticsContext'
import { useConfig } from '../ConfigProvider/ConfigProvider'
import getFeatures from './getFeatures'
import { UserContext } from './UserContext'

export { useUser } from './UserContext'

const initialTemperatureUnit = TemperatureUnit.fahrenheit

// Note: there is a similar UserProvider in packages/mobile/ui.
// We may want to deduplicate this in the future.
export function UserProvider({ configPrefix, children }) {
  const analytics = useAnalytics()
  const api = useApi()
  const snackbar = useSnackbar()

  const hasSavePreferencesFailedRef = useRef(false)
  const prevOptionsRef = useRef()
  const prevLocalOptionsRef = useRef()
  const prevLanguageRef = useRef()
  const prevLocalLanguageRef = useRef()
  const [user, setUser] = useState({
    isAuthenticated: false,
  })
  const config = useConfig()

  const appInsights = useAppInsightsContext()

  useEffect(() => {
    if (appInsights && appInsights.isInitialized()) {
      const instance = appInsights.getAppInsights()
      const telemetryInitializer = (envelope) => {
        envelope.tags['ai.user.authUserId'] = user?.id
      }
      instance.addTelemetryInitializer(telemetryInitializer)
    }
  }, [user?.id, appInsights])

  async function saveUserPreferences({
    endpoint = '/api/me/preferences',
    errMessage,
    payload,
  }) {
    try {
      let data = payload

      // Truncate to 10 recent assets to keep a reasonable data size.
      if (data?.profile?.recentAssets != null) {
        data = {
          ...data,
          profile: {
            ...data.profile,
            recentAssets: Object.fromEntries(
              Object.entries(data.profile.recentAssets).slice(-10)
            ),
          },
        }
      }
      await api.put(endpoint, data)
    } catch (err) {
      if (!hasSavePreferencesFailedRef.current) {
        hasSavePreferencesFailedRef.current = true
        snackbar.show(errMessage)
      }

      analytics.track('error', {
        message: err?.message,
        error: endpoint,
      })
    }
  }

  useEffect(() => {
    if (!user.isAuthenticated) return

    const isOptionNew =
      prevOptionsRef.current != null &&
      user.options != null &&
      !_.isEqual(prevOptionsRef.current, user.options)

    const isLanguageNew =
      user.language &&
      prevLanguageRef.current != null &&
      prevLanguageRef.current !== user.language

    if (isOptionNew || isLanguageNew) {
      saveUserPreferences({
        errMessage: 'An error has occurred while saving user profile data',
        payload: {
          /* operational view is a state shared with components
             rendered under both routes.sites__siteId and
             routes.portfolio, but it shouldn't be saved to
             user preference
          */
          profile: _.omit(user.options, [VIEW]),
          language: user.language,
        },
      })
    }
    prevLanguageRef.current = user.language
    prevOptionsRef.current = _.cloneDeep(user.options)
  }, [user.options, user.language])

  useEffect(() => {
    if (
      prevLocalOptionsRef.current != null &&
      user.localOptions != null &&
      !_.isEqual(prevLocalOptionsRef.current, user.localOptions)
    ) {
      localStorage.set(`${configPrefix}${user.id}`, user.localOptions)
    }

    prevLocalOptionsRef.current = _.cloneDeep(user.localOptions)
  }, [user.localOptions])

  useEffect(() => {
    if (
      prevLocalLanguageRef.current != null &&
      prevLocalLanguageRef.current !== user.localLanguage
    ) {
      localStorage.set(`${configPrefix}${user.id}.language`, user.localLanguage)
    }
    prevLocalLanguageRef.current = user.localLanguage
  }, [user.localLanguage])

  const context = {
    ...user,

    // Take currentConfig (the result of config.loadConfig()) as an argument
    // so we don't get a stale value when we `loadUser` directly after doing
    // a `config.loadConfig()`.
    // eslint-disable-next-line complexity
    async loadUser({ currentConfig, nextUser, meQueryError }) {
      try {
        if (meQueryError) {
          throw meQueryError
        }

        if (
          !currentConfig.isSingleTenant &&
          nextUser.customer?.status ===
            CustomerStatus.TRANSFERRED_TO_SINGLE_TENANT &&
          nextUser.customer?.singleTenantUrl != null &&
          window.location.pathname !== '/customer-transferred'
        ) {
          window.location.pathname = '/customer-transferred'
        }

        analytics.identify(nextUser.id, {
          Company: nextUser.company ?? '',
          Email: nextUser.email,
          Name: `${nextUser.firstName} ${nextUser.lastName}`,
          firstName: nextUser.firstName,
          lastName: nextUser.lastName,
          accountExternalID: nextUser.customer?.accountExternalId,
          contactExternalID: nextUser.email,
          customer_name: nextUser.customer?.name,
          isSingleTenant: currentConfig.isSingleTenant === true,
        })

        setUser({
          ...nextUser,
          isAuthenticated: true,
          ...(nextUser.customer != null
            ? {
                customer: {
                  ...nextUser.customer,
                  ...(nextUser.customer.features != null
                    ? {
                        features: getFeatures(nextUser.customer.features),
                      }
                    : {}),
                },
              }
            : {}),
          options: {
            ...(nextUser.preferences?.profile ?? {}),
            temperatureUnit:
              // initialise temperatureUnit value if not any
              nextUser.preferences?.profile?.temperatureUnit ||
              initialTemperatureUnit,
          },
          language: nextUser.preferences?.language || 'en',
          localOptions: localStorage.get(`${configPrefix}${nextUser.id}`) ?? {},
        })
      } catch (err) {
        // eslint-disable-next-line no-console
        console.error(err)

        if (err?.response?.status === 401 || err?.response?.status === 403) {
          setUser({
            isAuthenticated: false,
          })

          return
        }

        snackbar.show('An error has occurred retrieving your user details')
      }
    },

    logout() {
      analytics.track('Logged out')
      authService.logout(config)
    },

    saveOptions(key, value) {
      setUser((prevUser) => {
        const nextOptions = _.cloneDeep(prevUser.options)
        _.set(nextOptions, key, value)
        return {
          ...prevUser,
          options: nextOptions,
        }
      })
    },

    saveLanguage(value) {
      setUser((prevUser) => {
        return {
          ...prevUser,
          language: value,
        }
      })
    },

    saveLocalOptions(key, value) {
      setUser((prevUser) => {
        const nextLocalOptions = _.cloneDeep(prevUser.localOptions)
        _.set(nextLocalOptions, key, value)
        return {
          ...prevUser,
          localOptions: nextLocalOptions,
        }
      })
    },

    saveLocalLanguage(value) {
      setUser((prevUser) => {
        return {
          ...prevUser,
          localLanguage: value,
        }
      })
    },

    clearOptions(key) {
      setUser((prevUser) => ({
        ...prevUser,
        options: _.omit(_.cloneDeep(prevUser.options), key),
        localOptions: _.omit(_.cloneDeep(prevUser.localOptions), key),
      }))
    },

    clearAllOptions() {
      setUser((prevUser) => ({
        ...prevUser,
        options: {},
        localOptions: {},
      }))
    },

    /**
     * @param {Permission[] | Permission} permissions - The permissions to check.
     * @returns {boolean | undefined}
     * - `true` if the user has all the specified permissions.
     * - `undefined` if FGA feature is not enabled for the user.
     */
    hasPermissions(permissions) {
      if (!user?.policies) {
        return undefined
      }

      const permissionsToCheck =
        typeof permissions === 'string' ? [permissions] : permissions

      return permissionsToCheck.every((permission) =>
        user?.policies?.includes(permission)
      )
    },
  }

  return <UserContext.Provider value={context}>{children}</UserContext.Provider>
}
