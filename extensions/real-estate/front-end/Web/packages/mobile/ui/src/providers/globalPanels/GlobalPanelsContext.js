import { createContext, useContext } from 'react'

export const GlobalPanelsContext = createContext()

export function useGlobalPanels() {
  return useContext(GlobalPanelsContext)
}
