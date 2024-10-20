import { createContext, useContext } from 'react'

export const ScrollbarSizeContext = createContext()

export function useScrollbarSize() {
  return useContext(ScrollbarSizeContext)
}
