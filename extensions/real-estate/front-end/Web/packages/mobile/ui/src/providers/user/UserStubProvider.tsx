/* eslint-disable @typescript-eslint/no-empty-function */
import { PropsWithChildren, useMemo } from 'react'
import { v4 as uuidv4 } from 'uuid'
import { User, UserContext } from './UserContext'

export default function UserStubProvider({
  children,
  options = undefined,
}: PropsWithChildren<{ options: Partial<User> | undefined }>) {
  const context = useMemo(
    () => ({
      // Lots more user properties can go here
      id: uuidv4(),
      firstName: 'Test123',
      lastName: 'Last',
      initials: 'TL',
      email: 'testing123@willowinc.com',
      sites: [],
      options: {},
      isAuthenticated: true,
      loadUser: () => {},
      logout: () => {},
      saveUserOptions: () => {},
      clearUserOptions: () => {},
      ...options,
    }),
    [options]
  )
  return <UserContext.Provider value={context}>{children}</UserContext.Provider>
}
