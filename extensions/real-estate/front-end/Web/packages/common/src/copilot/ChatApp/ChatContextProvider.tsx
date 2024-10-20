import { ReactNode, useCallback, useReducer } from 'react'
import { v4 as uuidv4 } from 'uuid'
import { ChatState, ChatContext, Message } from './ChatContext'

export enum ChatsActionType {
  'onSessionReset',
  'onToggleLauncher',
  'onSetUserMessage',
  'onSetMessages',
}

export type ChatsAction =
  | {
      type: ChatsActionType.onSessionReset
      copilotSessionId: string
    }
  | {
      type: ChatsActionType.onToggleLauncher
      isActive: boolean
    }
  | { type: ChatsActionType.onSetUserMessage; userMessage: string }
  | { type: ChatsActionType.onSetMessages; messages?: Message[] }

const defaultState: ChatState = {
  copilotSessionId: uuidv4(),
  content: '',
  isActive: false,
}

const chatsReducer = (state: ChatState, action: ChatsAction): ChatState => {
  switch (action.type) {
    case ChatsActionType.onToggleLauncher:
      return { ...state, isActive: action.isActive, userMessage: undefined }
    case ChatsActionType.onSetUserMessage:
      return { ...state, userMessage: action.userMessage }
    case ChatsActionType.onSetMessages:
      return { ...state, messages: action.messages }
    case ChatsActionType.onSessionReset:
      return { ...state, copilotSessionId: action.copilotSessionId }
    default:
      return state
  }
}

export default function ChatContextProvider({
  children,
}: {
  children: ReactNode
}) {
  const [
    { content, isActive, userMessage, messages, copilotSessionId },
    dispatch,
  ] = useReducer(chatsReducer, defaultState)

  const onToggle = useCallback(
    (currActive: boolean) =>
      dispatch({
        type: ChatsActionType.onToggleLauncher,
        isActive: currActive,
      }),
    []
  )

  const onSetUserMessage = useCallback(
    (currUserMsg: string) =>
      dispatch({
        type: ChatsActionType.onSetUserMessage,
        userMessage: currUserMsg,
      }),
    []
  )

  const onSetMessages = useCallback(
    ({ prevMessages, newUserMsg, response }) =>
      dispatch({
        type: ChatsActionType.onSetMessages,
        messages: [
          ...prevMessages,
          { content: newUserMsg, id: uuidv4() },
          response,
        ],
      }),
    []
  )

  const onResetCopilotSession = useCallback(
    () =>
      dispatch({
        type: ChatsActionType.onSessionReset,
        copilotSessionId: uuidv4(),
      }),
    []
  )

  const context = {
    content,
    isActive,
    onToggle,
    onSetUserMessage,
    userMessage,
    messages,
    onSetMessages,
    onResetCopilotSession,
    copilotSessionId,
  }

  return <ChatContext.Provider value={context}>{children}</ChatContext.Provider>
}
