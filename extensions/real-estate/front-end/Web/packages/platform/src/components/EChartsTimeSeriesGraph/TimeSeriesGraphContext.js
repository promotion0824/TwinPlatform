import { createContext, useContext } from 'react'

export const TimeSeriesGraphContext = createContext()

export function useTimeSeriesGraph() {
  return useContext(TimeSeriesGraphContext)
}
