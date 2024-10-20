import { useQuery, UseQueryOptions } from 'react-query'
import {
  getTicketCategories,
  TicketCategoriesResponse,
} from '../../services/Tickets/TicketsService'

/**
 * Fetch ticket categories data for creating or updating existing ticket.
 */
export default function useGetTicketCategories(
  { isEnabled }: { isEnabled: boolean },
  options?: UseQueryOptions<TicketCategoriesResponse>
) {
  return useQuery(['tickets-categories'], () => getTicketCategories(), {
    enabled: isEnabled,
    ...options,
  })
}
