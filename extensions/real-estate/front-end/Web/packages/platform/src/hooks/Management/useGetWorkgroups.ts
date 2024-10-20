import { api } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'

export default function useGetWorkgroups(
  siteId: string,
  options: UseQueryOptions
) {
  return useQuery(
    ['workgroups', siteId],
    async () => {
      try {
        const response = await api.get(`/management/sites/${siteId}/workgroups`)
        return response.data
      } catch (_) {
        // We're okay with this route failing (at this time),
        // and will simply return an empty array.
        return []
      }
    },
    options
  )
}
