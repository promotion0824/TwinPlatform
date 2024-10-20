import { createContext, useContext } from 'react'

export type User = {
  id: string
  firstName: string
  lastName: string
  initials: string
  email: string
  company?: string
  accountExternalId?: string
  customerId?: string
  sites: {
    id: string
    name: string
    address: string
    timeZone: string
    accountExternalId: string
    customerId: string
    features: {
      isTicketingDisabled: boolean
      isInsightsDisabled: boolean
      is2DViewerDisabled: boolean
      isReportsEnabled: boolean
      is3DAutoOffsetEnabled: boolean
      isInspectionEnabled: boolean
      isScheduledTicketsEnabled: boolean
    }
  }[]
  isAuthenticated: boolean
  options: any
  loadUser: () => void
  logout: () => void
  saveUserOptions: (key: string, value: any) => void
  clearUserOptions: (key: string) => void
}

export const UserContext = createContext<User | undefined>(undefined)

export function useUser() {
  const user = useContext(UserContext)
  if (user == null) {
    throw new Error(`useUser needs a UserProvider`)
  }
  return user
}
