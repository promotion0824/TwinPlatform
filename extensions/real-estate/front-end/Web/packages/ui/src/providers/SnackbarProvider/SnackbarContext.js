import { createContext, useContext } from 'react'

export const SnackbarContext = createContext()

/**
 * @deprecated Please do not use this hook anymore.
 * Use the `useSnackbar` hook from `@willowinc/ui` instead.
 */
export function useSnackbar() {
  return useContext(SnackbarContext)
}
