import { createContext, useContext } from 'react'
import { ProviderRequiredError } from '@willow/common'

export type ChatResponse = {
  citations: Array<{ name: string; page: string }>
  responseText: string
  content?: string
  isError?: boolean
}

export type Message = {
  id: number
  content: string
  citations: Array<{ name: string; pages: string[] }>
  showCitation: boolean
  isCopilot?: boolean
  isError?: boolean
}

export interface ChatState {
  content: string
  isCopilot?: boolean
  isActive: boolean
  messages?: Message[]
  userMessage?: string
  copilotSessionId: string
}

export interface ChatContextType extends ChatState {
  onResetCopilotSession: () => void
  onSetMessages: ({
    prevMessages,
    newUserMsg,
    response,
  }: {
    prevMessages: Message[]
    newUserMsg: string
    response?: ChatResponse
  }) => void
  onToggle: (currActive: boolean) => void
  onSetUserMessage: (currUserMsg: string) => void
}

export const ChatContext = createContext<ChatContextType | undefined>(undefined)

export function useChat() {
  const context = useContext(ChatContext)
  if (context == null) {
    throw new ProviderRequiredError('CopilotChat')
  }
  return context
}
