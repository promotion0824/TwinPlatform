import { SnackbarContext } from './SnackbarContext'

export default function SnackbarProvider({ children }) {
  const context = {
    snackbars: [],
    show: (header, description) => {},
    hide: (snackbarId) => {},
    clear: () => {},
  }

  return (
    <SnackbarContext.Provider value={context}>
      {children}
    </SnackbarContext.Provider>
  )
}
