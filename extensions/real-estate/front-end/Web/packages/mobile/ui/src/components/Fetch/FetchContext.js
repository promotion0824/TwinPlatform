import { createContext, useContext } from 'react'

export const FetchContext = createContext()

export function useFetch() {
  return useContext(FetchContext)
}
