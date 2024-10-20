import { createContext, useContext } from 'react'

export const LabelContext = createContext()

export function useLabel() {
  return useContext(LabelContext)
}
