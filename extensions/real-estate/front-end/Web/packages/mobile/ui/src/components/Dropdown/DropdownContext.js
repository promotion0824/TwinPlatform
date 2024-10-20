import { createContext, useContext } from 'react'

export const DropdownContext = createContext()

export function useDropdown() {
  return useContext(DropdownContext)
}
