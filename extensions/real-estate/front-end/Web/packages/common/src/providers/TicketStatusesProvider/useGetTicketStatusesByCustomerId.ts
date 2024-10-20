import { useQuery } from 'react-query'
import { TicketStatus } from '../../ticketStatus/types'

export default function useGetTicketStatusesByCustomerId(
  customerId: string,
  getTicketStatuses: (customerId: string) => Promise<TicketStatus[]>
) {
  return useQuery(
    ['customer', customerId, 'ticketStatuses'],
    () => getTicketStatuses(customerId),
    {
      enabled: customerId != null,
    }
  )
}
