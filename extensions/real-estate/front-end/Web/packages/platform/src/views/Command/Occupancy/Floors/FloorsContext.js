import { createContext, useContext } from 'react'

export const FloorsContext = createContext()

export function useFloors() {
  return useContext(FloorsContext)
}
