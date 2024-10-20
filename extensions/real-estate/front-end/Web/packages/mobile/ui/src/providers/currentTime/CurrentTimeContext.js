import { createContext, useContext } from 'react'

export const CurrentTimeContext = createContext()

export function useCurrentTime() {
  return useContext(CurrentTimeContext)
}
