/* eslint-disable @typescript-eslint/no-empty-function */
import { SnackbarContext } from './SnackbarContext'

/**
 * Stub version of SnackbarProvider
 */
export default function SnackbarStubProvider({ children }) {
  const context = {
    show: (message, options) => {},
    hide: ({ snackbarId, isToast = false }) => {},
    clear: () => {},
    close: ({ snackbarId, isToast = false }) => {},
  }

  return (
    <SnackbarContext.Provider value={context}>
      {children}
    </SnackbarContext.Provider>
  )
}
