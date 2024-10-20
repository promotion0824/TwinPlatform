import { createContext, useContext } from 'react'

export const MapContext = createContext()

export function useMap() {
  return useContext(MapContext)
}
