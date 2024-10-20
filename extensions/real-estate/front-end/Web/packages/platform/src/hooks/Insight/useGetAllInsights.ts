import { useQuery, UseQueryOptions } from 'react-query'
import {
  Specifications,
  AllInsightsResponse,
  fetchAllInsights,
} from '../../services/Insight/InsightsService'

export default function useGetAllInsights(
  params: Specifications,
  options?: UseQueryOptions<AllInsightsResponse>
) {
  return useQuery(
    ['all-insights', params],
    () =>
      fetchAllInsights({
        params,
      }),
    options
  )
}
