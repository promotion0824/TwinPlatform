import { useEffect, useState } from 'react'
import _ from 'lodash'
import { useApi } from 'hooks'
import { useSnackbar } from 'providers/snackbar/SnackbarContext'
import { authService, localStorage } from '@willow/common'
import { UserContext } from './UserContext'
import { useAnalytics } from '../analytics/AnalyticsContext'
import { useConfig } from '../config/ConfigContext'
import { CustomerStatus } from '@willow/common/constants'

const initialState = {
  isAuthenticated: false,
}

export { default as useUserState } from './useUserState'

// Note: there is a similar UserProvider in packages/ui.
// We may want to deduplicate this in the future.
export default function UserProvider({ configPrefix, url, ...rest }) {
  const analytics = useAnalytics()
  const snackbar = useSnackbar()
  const api = useApi()
  const config = useConfig()

  const [user, setUser] = useState(initialState)

  useEffect(() => {
    if (user.id) {
      localStorage.set(`${configPrefix}${user.id}`, user.options)
    }
  }, [user.options])

  const context = {
    ...user,

    // Take currentConfig (the result of config.loadConfig()) as an argument
    // so we don't get a stale value when we `loadUser` directly after doing
    // a `config.loadConfig()`.
    async loadUser(currentConfig) {
      try {
        const nextUser = await api.get(url, undefined, { cache: true })

        if (
          !currentConfig.isSingleTenant &&
          nextUser.customer?.status ===
            CustomerStatus.TRANSFERRED_TO_SINGLE_TENANT &&
          nextUser.customer.singleTenantUrl != null &&
          window.location.pathname !== '/customer-transferred'
        ) {
          window.location.pathname = '/customer-transferred'
        }

        const options = localStorage.get(`${configPrefix}${nextUser.id}`) ?? {}

        analytics.identify(nextUser.id, {
          Company: nextUser.company ?? '',
          Email: nextUser.email,
          Name: `${nextUser.firstName} ${nextUser.lastName}`,
          firstName: nextUser.firstName,
          lastName: nextUser.lastName,
          accountExternalID: nextUser.accountExternalId,
          contactExternalID: nextUser.email,
          isSingleTenant: currentConfig.isSingleTenant === true,
        })

        setUser({
          ...nextUser,
          isAuthenticated: true,
          options,
        })
      } catch (err) {
        if (err.response?.status === 401) {
          setUser({
            isAuthenticated: false,
          })

          return
        }
        snackbar.show('An error has occurred retrieving your user details.')
      }
    },

    async logout() {
      try {
        await api.post('/api/signout')
        analytics.track('Logged out')

        authService.logout(config)
      } catch (err) {
        snackbar.show('An error has occurred while signing out')
      }
    },

    saveUserOptions(key, value) {
      setUser((prevUser) => {
        const nextOptions = _.cloneDeep(prevUser.options)
        _.set(nextOptions, key, value)

        return {
          ...prevUser,
          options: nextOptions,
        }
      })
    },

    clearUserOptions(key) {
      setUser((prevUser) => ({
        ...prevUser,
        options: _.omit(_.cloneDeep(prevUser.options), key),
      }))
    },
  }

  return <UserContext.Provider {...rest} value={context} />
}
