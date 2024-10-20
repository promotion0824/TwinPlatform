import { createContext, useContext } from 'react'

export const ModalsContext = createContext()

export function useModals() {
  return useContext(ModalsContext)
}
