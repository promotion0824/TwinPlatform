import { useQuery, UseQueryOptions } from 'react-query'
import { api } from '@willow/ui'
import { CardSummaryFilters } from '@willow/common/insights/insights/types'

export default function useGetInsightFilters(
  params: {
    siteIds?: string[]
    statusList?: string[] | string
    scopeId?: string
  },
  options?: UseQueryOptions<{
    filters: CardSummaryFilters
  }>
) {
  return useQuery(
    ['insight-filters', params],
    async () => {
      const response = await api.post('/insights/filters', {
        ...params,
      })
      return response.data
    },
    options
  )
}
