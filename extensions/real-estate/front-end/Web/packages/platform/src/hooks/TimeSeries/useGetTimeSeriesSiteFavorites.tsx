import { api } from '@willow/ui'
import { UseQueryOptions, useQuery } from 'react-query'

import type { TimeSeriesFavorite } from '../../views/TimeSeries/types'

type SiteFavoritesQuery = { favorites?: TimeSeriesFavorite[] }

export default function useGetTimeSeriesSiteFavorites(
  siteId: string,
  options?: UseQueryOptions<SiteFavoritesQuery>
) {
  return useQuery<SiteFavoritesQuery>(
    ['timeSeriesSiteFavorites', siteId],
    async () => {
      const response = await api.get(`/sites/${siteId}/preferences/timeMachine`)
      return response.data
    },
    options
  )
}
