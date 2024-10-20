import { useEffect } from 'react'
import { useApi, useUser, Loader } from '@willow/mobile-ui'
import { useDeviceId } from 'providers'
import styles from './AccountLayout/AccountLayout.css'

export default function Logout() {
  const api = useApi()
  const { isAuthenticated, logout } = useUser()
  const { deviceId } = useDeviceId()

  useEffect(() => {
    async function handleLogout() {
      if (deviceId != null) {
        try {
          await api.delete(`/api/installations?pnsHandle=${deviceId}`)
        } catch (err) {
          // do nothing
        }
      }

      await logout()
    }

    if (isAuthenticated) {
      handleLogout()
    }
  }, [isAuthenticated])

  return (
    <div className={styles.logoutContent}>
      Logout <Loader className={styles.logoutLoader} />
    </div>
  )
}
