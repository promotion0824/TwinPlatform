import { useQuery, UseQueryOptions } from 'react-query'
import {
  InsightsResponse,
  fetchAssetInsights,
  Specifications,
} from '../../services/Insight/InsightsService'

export default function useGetAssetInsights(
  params: Specifications,
  options?: UseQueryOptions<InsightsResponse>
) {
  return useQuery(
    ['asset-insights', params],
    () =>
      fetchAssetInsights({
        params,
      }),
    options
  )
}
