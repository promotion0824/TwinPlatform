import { useQuery, UseQueryOptions } from 'react-query'
import { api } from '@willow/ui'

export type TicketSubStatus = {
  id: string
  name: string
}
/**
 * Fetch ticket sub status
 */
export default function useGetTicketSubStatus(
  options?: UseQueryOptions<TicketSubStatus[]>
) {
  return useQuery(
    ['tickets-subStatus'],
    async () => {
      const response = await api.get('/tickets/subStatus')
      return response.data
    },
    {
      staleTime: 1000 * 60 * 60 * 1, // 1 hour
      refetchOnMount: 'always',
      ...options,
    }
  )
}
