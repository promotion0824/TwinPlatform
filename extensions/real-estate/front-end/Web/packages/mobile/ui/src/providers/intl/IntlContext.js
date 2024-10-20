import { createContext, useContext } from 'react'

export const IntlContext = createContext()

export function useIntl() {
  return useContext(IntlContext)
}
