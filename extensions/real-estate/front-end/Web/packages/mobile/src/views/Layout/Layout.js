import { useEffect } from 'react'
import { useUser, Spacing, NotFound } from '@willow/mobile-ui'
import { useDeviceId } from 'providers'
import LayoutComponent from './Layout/Layout'
import SetupNativeNotifications from './SetupNativeNotifications'
import styles from './Layout.css'

export { default as LayoutHeader } from './Layout/LayoutHeader'

export default function Layout({ children }) {
  const { sites } = useUser()
  const { deviceId, getDeviceId } = useDeviceId()

  useEffect(() => {
    getDeviceId()
  }, [])

  if (deviceId === undefined) {
    return null
  }

  return (
    <>
      <Spacing position="fixed" type="content" className={styles.layout}>
        <LayoutComponent sites={sites}>
          {sites.length === 0 && <NotFound>No sites found</NotFound>}
          {sites.length > 0 && children}
        </LayoutComponent>
      </Spacing>
      <SetupNativeNotifications />
    </>
  )
}
