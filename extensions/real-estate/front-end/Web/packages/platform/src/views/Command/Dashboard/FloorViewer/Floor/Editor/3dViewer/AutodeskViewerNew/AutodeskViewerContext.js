import { createContext, useContext } from 'react'

export const AutodeskViewerContext = createContext()

export function useAutodeskViewer() {
  return useContext(AutodeskViewerContext)
}
