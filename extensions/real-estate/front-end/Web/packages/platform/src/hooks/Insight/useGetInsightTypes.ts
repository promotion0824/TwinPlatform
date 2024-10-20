import { useQuery, UseQueryOptions } from 'react-query'
import { InsightTypesDto } from '@willow/common/insights/insights/types'
import {
  fetchInsightTypes,
  Specifications,
} from '../../services/Insight/InsightsService'

export default function useGetInsightTypes(
  params: Specifications,
  options?: UseQueryOptions<InsightTypesDto>
) {
  return useQuery(
    ['insight-types', params],
    () =>
      fetchInsightTypes({
        params,
      }),
    {
      ...options,
      enabled: options?.enabled !== false,
    }
  )
}
