import { createContext, useContext } from 'react'

export const TimeSeriesContext = createContext()

export function useTimeSeries() {
  return useContext(TimeSeriesContext)
}
