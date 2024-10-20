import { SnackbarContext } from './SnackbarContext'

export default function SnackbarStubProvider({ children }) {
  return (
    <SnackbarContext.Provider
      value={{
        show: (message, options) => {},
        hide: (snackbarId) => {},
        clear: () => {},
        close: (snackbarId) => {},
      }}
    >
      {children}
    </SnackbarContext.Provider>
  )
}
