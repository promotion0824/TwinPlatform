import { createContext, useContext } from 'react'

export const SelectContext = createContext()

export function useSelect() {
  return useContext(SelectContext)
}
