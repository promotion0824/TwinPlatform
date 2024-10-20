import { useQuery, UseQueryOptions } from 'react-query'
import {
  InsightsResponse,
  fetchInsights,
  Specifications,
} from '../../services/Insight/InsightsService'

/**
 * Retrieve insights for either a single site or for all sites.
 */
export default function useGetInsights(
  params: Specifications,
  options?: UseQueryOptions<InsightsResponse>
) {
  return useQuery(
    ['insights', params],
    () => fetchInsights({ specifications: params }),
    options
  )
}
