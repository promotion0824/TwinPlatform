import { useQuery, UseQueryOptions } from 'react-query'
import { api } from '@willow/ui'
import { ChatResponse } from '@willow/common/copilot/ChatApp/ChatContext'

export default function useGetChatResponse(
  params: {
    userInput: string
    options?: { runFlags: string[] }
    context: { sessionId: string; userId?: string }
  },
  options?: UseQueryOptions<ChatResponse>
) {
  return useQuery(
    ['chat-response', params],
    async () => {
      const response = await api.post(`/chat`, {
        ...params,
      })
      return response.data
    },
    options
  )
}
