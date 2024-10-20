import { api } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'

type ActiveInsightsCountByTwinsModelResponse = Array<{
  count: number
  modelId: string
}>

export default function useGetActiveInsightCountsByTwinModel({
  limit,
  options,
  twinId,
}: {
  limit?: number
  options?: UseQueryOptions<ActiveInsightsCountByTwinsModelResponse>
  twinId: string
}) {
  return useQuery(
    ['activeInsightCountsByTwinModel', twinId, limit],
    async () => {
      const response = await api.get(
        `/insights/twin/${twinId}/activeInsightCountsByTwinModel`,
        {
          params: { limit },
        }
      )
      return response.data
    },
    options
  )
}
