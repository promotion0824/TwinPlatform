import { createContext, useContext } from 'react'

export const DashboardContext = createContext()

export function useDashboard() {
  return useContext(DashboardContext)
}
