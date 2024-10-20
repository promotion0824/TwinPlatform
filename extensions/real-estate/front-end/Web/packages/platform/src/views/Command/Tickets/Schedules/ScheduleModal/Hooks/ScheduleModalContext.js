import { createContext, useContext } from 'react'

export const ScheduleModalContext = createContext()

export function useScheduleModal() {
  return useContext(ScheduleModalContext)
}
