import { useState } from 'react'
import { native } from 'utils'
import { DeviceIdContext } from './DeviceIdContext'

export default function DeviceIdProvider({ children }) {
  const [deviceId, setDeviceId] = useState()

  const context = {
    deviceId,

    async getDeviceId() {
      const nextDeviceId = await native.getDeviceId()

      setDeviceId(nextDeviceId)
    },
  }

  return (
    <DeviceIdContext.Provider value={context}>
      {children}
    </DeviceIdContext.Provider>
  )
}
