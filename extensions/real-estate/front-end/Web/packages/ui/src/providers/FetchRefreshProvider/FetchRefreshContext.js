import { createContext, useContext } from 'react'

export const FetchRefreshContext = createContext()

export function useFetchRefresh() {
  return useContext(FetchRefreshContext)
}
