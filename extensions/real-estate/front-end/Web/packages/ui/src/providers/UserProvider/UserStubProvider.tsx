import { TemperatureUnit } from '@willow/common'
import { ReactNode } from 'react'
import { UserContext } from './UserContext'

export default function UserStubProvider({
  children,
  options,
}: {
  children: ReactNode
  options?: {
    saveOptions?: (key, value) => void
    customer?: {
      id: string
      name: string
    }
    portfolios?: Array<{
      features?: Record<string, boolean>
      id: string
      name?: string
    }>
  }
}) {
  return (
    <UserContext.Provider
      value={{
        // Lots more user properties can go here
        customer: {
          name: 'My customer',
          id: 'customer-id-123',
        },
        loadUser: () => {},
        logout: () => {},
        saveOptions: (key, value) => {},
        saveLanguage: (value) => {},
        saveLocalOptions: (key, value) => {},
        saveLocalLanguage: (value) => {},
        clearOptions: (key) => {},
        clearAllOptions: () => {},
        options: {
          temperatureUnit: TemperatureUnit.celsius,
        },
        ...options,
      }}
    >
      {children}
    </UserContext.Provider>
  )
}
