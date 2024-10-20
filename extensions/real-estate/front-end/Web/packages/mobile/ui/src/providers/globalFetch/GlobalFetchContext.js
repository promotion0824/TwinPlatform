import { createContext, useContext } from 'react'

export const GlobalFetchContext = createContext()

export function useGlobalFetch() {
  return useContext(GlobalFetchContext)
}
