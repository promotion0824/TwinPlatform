import { createContext, useContext } from 'react'

export const TicketsContext = createContext()

export function useTickets() {
  return useContext(TicketsContext)
}
