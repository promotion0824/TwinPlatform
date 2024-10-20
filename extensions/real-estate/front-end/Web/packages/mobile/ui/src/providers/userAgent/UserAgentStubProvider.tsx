import { UserAgentContext } from './UserAgentContext'

export default function UserAgentProvider({ children }) {
  const context = {
    isIpad: false,
  }

  return (
    <UserAgentContext.Provider value={context}>
      {children}
    </UserAgentContext.Provider>
  )
}
