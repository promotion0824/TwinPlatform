import { createContext, useContext } from 'react'

export const UserAgentContext = createContext()

export function useUserAgent() {
  return useContext(UserAgentContext)
}
