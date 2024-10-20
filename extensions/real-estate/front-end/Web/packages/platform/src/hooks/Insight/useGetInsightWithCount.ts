import { useQuery, UseQueryOptions } from 'react-query'
import {
  InsightsWithTotalCount,
  Specifications,
  fetchInsightsWithCount,
} from '../../services/Insight/InsightsService'

/**
 * Retrieve insights for either a single site or for all sites with total count.
 */
export default function useGetInsightWithCount(
  params: Specifications,
  options?: UseQueryOptions<InsightsWithTotalCount>
) {
  return useQuery(
    ['insights', params],
    () => fetchInsightsWithCount({ specifications: params }),
    options
  )
}
