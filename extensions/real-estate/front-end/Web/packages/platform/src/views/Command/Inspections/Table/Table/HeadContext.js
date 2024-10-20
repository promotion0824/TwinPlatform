import { createContext, useContext } from 'react'

export const HeadContext = createContext()

export function useHead() {
  return useContext(HeadContext)
}
