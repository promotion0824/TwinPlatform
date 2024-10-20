import { api } from '@willow/ui'
import { QueryClient, useQuery } from 'react-query'

import type { TimeSeriesFavorite } from '../types'

const getScopeFavoritesUrl = (scopeId: string) =>
  `/scopes/${scopeId}/preferences/timeMachine`

const timeSeriesScopeFavoritesQueryKey = 'timeSeriesScopeFavorites'

/** The Query will not be invoked if scopeId is undefined. */
export function useTimeSeriesScopeFavorites(scopeId?: string) {
  return useQuery<{
    favorites?: TimeSeriesFavorite[]
  }>(
    [timeSeriesScopeFavoritesQueryKey, scopeId],
    async () => {
      const response = await api.get(
        getScopeFavoritesUrl(
          scopeId! /* query will be disabled if no scopeId as below */
        )
      )
      return response.data
    },
    {
      enabled: !!scopeId,
    }
  )
}

export async function updateTimeSeriesScopeFavorites(
  scopeId: string,
  updatedFavorites: TimeSeriesFavorite[] | undefined,
  queryClient: QueryClient
) {
  try {
    await api.put(getScopeFavoritesUrl(scopeId), {
      favorites: updatedFavorites,
    })

    queryClient.invalidateQueries([timeSeriesScopeFavoritesQueryKey, scopeId])
  } catch (err) {
    throw new Error(err)
  }
}
