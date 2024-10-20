import { useQuery, UseQueryOptions } from 'react-query'
import {
  getTickets,
  TicketsParams,
  TicketsResponse,
} from '../../services/Tickets/TicketsService'

/**
 * Fetch tickets.
 * When assetId is provided, fetch tickets based on asset.
 * When siteId is provided, fetch tickets for that site. Otherwise, fetch all tickets.
 */
export default function useGetTickets(
  params: Partial<TicketsParams>,
  options?: UseQueryOptions<TicketsResponse>
) {
  return useQuery(['tickets', params], () => getTickets(params), options)
}
