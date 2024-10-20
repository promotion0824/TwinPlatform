import { createContext, useContext } from 'react'

export const TypeaheadContext = createContext()

export function useTypeahead() {
  return useContext(TypeaheadContext)
}
