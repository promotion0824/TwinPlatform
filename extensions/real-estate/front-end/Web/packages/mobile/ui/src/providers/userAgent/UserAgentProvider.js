import { UserAgentContext } from './UserAgentContext'

/**
 * Stub version of UserAgentProvider
 */
export default function UserAgentStubProvider({ children }) {
  const context = {
    isIpad: window.navigator.userAgent.includes('iPad'),
  }

  return (
    <UserAgentContext.Provider value={context}>
      {children}
    </UserAgentContext.Provider>
  )
}
