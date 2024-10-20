import { createContext, useContext } from 'react'

export const GraphContext = createContext()

export function useGraph() {
  return useContext(GraphContext)
}
