import { useEffect } from 'react'
import { native } from 'utils'
import { useApi } from '@willow/mobile-ui'
import { useDeviceId } from 'providers'

export default function SetupNativeNotifications({ children }) {
  const api = useApi()
  const { deviceId } = useDeviceId()

  useEffect(() => {
    const nativeType = native.getNativeType()

    if (
      deviceId != null &&
      (nativeType === 'ios' || nativeType === 'android')
    ) {
      let platform
      if (nativeType === 'ios') platform = 'apns'
      if (nativeType === 'android') platform = 'fcm'

      api.post('/api/installations', {
        platform,
        handle: deviceId,
      })
    }
  }, [])

  return children ?? null
}
