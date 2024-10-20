import { QueryClient, useQuery, UseQueryOptions } from 'react-query'

import { api } from '@willow/ui'

import { TwinResponse } from '../types'

type TwinRef = { siteId?: string; twinId: string }

function getTwinQueryKey({ siteId, twinId }: TwinRef) {
  return ['twin', siteId, twinId]
}

type GetTwinResponse = {
  twin: TwinResponse
  permissions: {
    edit: boolean
  }
}

export function useGetTwin(
  { siteId, twinId }: TwinRef,
  options?: UseQueryOptions<GetTwinResponse>
) {
  return useQuery<GetTwinResponse>(
    getTwinQueryKey({ siteId, twinId }),
    async () => {
      const response = await api.get(
        siteId ? `/v2/sites/${siteId}/twins/${twinId}` : `/v2/twins/${twinId}`
      )

      return response.data
    },
    {
      enabled: !!twinId,
      ...options,
    }
  )
}

export function invalidateTwin(
  queryClient: QueryClient,
  { siteId, twinId }: TwinRef
) {
  queryClient.invalidateQueries(getTwinQueryKey({ siteId, twinId }))
}
