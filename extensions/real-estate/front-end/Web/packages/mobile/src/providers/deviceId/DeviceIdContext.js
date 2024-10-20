import { createContext, useContext } from 'react'

export const DeviceIdContext = createContext()

export function useDeviceId() {
  return useContext(DeviceIdContext)
}
