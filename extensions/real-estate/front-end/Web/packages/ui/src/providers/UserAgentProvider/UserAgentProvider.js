import { UserAgentContext } from './UserAgentContext'

export { useUserAgent } from './UserAgentContext'

export function UserAgentProvider({ children }) {
  const context = {
    isIpad: window.navigator.userAgent.includes('iPad'),
  }

  return (
    <UserAgentContext.Provider value={context}>
      {children}
    </UserAgentContext.Provider>
  )
}
