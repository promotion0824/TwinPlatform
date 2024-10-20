import { createContext, useContext } from 'react'

export const InputContext = createContext()

export function useInput() {
  return useContext(InputContext)
}
