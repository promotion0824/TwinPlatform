import { ReactNode } from 'react'
import { noop } from 'lodash'
import { ChatContext } from './ChatContext'

export default function ChatAppStubProvider({
  children,
}: {
  children?: ReactNode
}) {
  return (
    <ChatContext.Provider
      value={{
        onResetCopilotSession: noop,
        onSetMessages: noop,
        onToggle: noop,
        onSetUserMessage: noop,
        copilotSessionId: '',
        isActive: false,
        content: '',
      }}
    >
      {children}
    </ChatContext.Provider>
  )
}
