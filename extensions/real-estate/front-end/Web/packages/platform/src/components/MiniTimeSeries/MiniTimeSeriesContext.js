import { createContext, useContext } from 'react'

export const MiniTimeSeriesContext = createContext()

export function useMiniTimeSeries() {
  return useContext(MiniTimeSeriesContext)
}
