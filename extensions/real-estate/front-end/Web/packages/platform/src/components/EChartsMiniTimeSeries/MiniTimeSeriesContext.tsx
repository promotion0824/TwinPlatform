import { createContext, useContext } from 'react'

export const MiniTimeSeriesContext = createContext(null)

export function useMiniTimeSeries() {
  return useContext(MiniTimeSeriesContext)
}
