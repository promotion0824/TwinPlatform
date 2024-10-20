import { useQuery, UseQueryOptions } from 'react-query'
import { api } from '@willow/ui'
import { InsightDiagnosticResponse } from '../../services/Tickets/TicketsService'

/**
 * Fetch dependent diagnostic insight details when creating a new ticket
 */
export default function useGetInsightDiagnostics(
  insightId: string,
  options?: UseQueryOptions<InsightDiagnosticResponse>
) {
  return useQuery(
    ['insight-diagnostic', insightId],
    async () => {
      const response = await api.get(
        `/insights/${insightId}/diagnostics/snapshot`
      )
      return response.data
    },
    options
  )
}
