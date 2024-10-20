import { createContext, useContext } from 'react'

export const PanelsContext = createContext()

export function usePanels() {
  return useContext(PanelsContext)
}
