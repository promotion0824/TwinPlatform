import {
  getAuthConfigFromLocalStorage,
  getAuthConfigKey,
  useInterval,
} from '@willow/common'
import { AxiosInstance } from 'axios'
import { useHistory } from 'react-router'
import routes from '../../../../platform/src/routes'

const minutes = 60 * 1000
const SILENT_RENEW_CHECK_INTERVAL = 5 * minutes
const EXPIRY_FAILURE_THRESHOLD = 10 * minutes

export default function AuthSilentRenew({
  api,
  app,
  useUser,
}: {
  api: AxiosInstance
  app: 'mobile' | 'platform'
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  useUser: () => any
}) {
  const history = useHistory()
  const user = useUser()

  useInterval(async () => {
    try {
      await api.post(`${app === 'platform' ? '/me' : ''}/refreshSession`)
    } catch {
      const authConfig = getAuthConfigFromLocalStorage(user.id)
      const expiresAt = authConfig?.expiresAt ?? Date.now()

      if (expiresAt - Date.now() < EXPIRY_FAILURE_THRESHOLD) {
        window.localStorage.removeItem(getAuthConfigKey(user.id))
        // Setting a small timeout to give localStorage.removeItem time to complete
        setTimeout(() => history.push(routes.account_idle_timeout), 100)
      }
    }
  }, SILENT_RENEW_CHECK_INTERVAL)

  return null
}
