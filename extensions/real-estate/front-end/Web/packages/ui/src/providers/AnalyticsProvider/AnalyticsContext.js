import { createContext, useContext } from 'react'

export const AnalyticsContext = createContext()

export function useAnalytics() {
  return useContext(AnalyticsContext)
}
