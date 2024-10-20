import { createContext, useContext } from 'react'

export const OnClickOutsideIdsContext = createContext()

export function useOnClickOutsideIds() {
  return useContext(OnClickOutsideIdsContext)
}
