import { createContext, useContext } from 'react'

export const LayoutContext = createContext()

export function useLayout() {
  return useContext(LayoutContext)
}
