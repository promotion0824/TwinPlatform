import { createContext, useContext } from 'react'

export const FloorContext = createContext()

export function useFloor() {
  return useContext(FloorContext)
}
