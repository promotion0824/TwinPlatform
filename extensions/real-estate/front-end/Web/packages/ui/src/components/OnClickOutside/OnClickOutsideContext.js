import { createContext, useContext } from 'react'

export const OnClickOutsideContext = createContext()

export function useOnClickOutside() {
  return useContext(OnClickOutsideContext)
}
