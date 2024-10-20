import { createContext, useContext } from 'react'

export const InspectionsContext = createContext()

export function useInspections() {
  return useContext(InspectionsContext)
}
