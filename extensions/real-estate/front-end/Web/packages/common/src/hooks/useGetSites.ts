import { api } from '@willow/ui'
import _ from 'lodash'
import { useQuery, UseQueryOptions } from 'react-query'
import { Site } from '../site/site/types'

/**
 * return a list of sites
 */
export default function useGetSites(
  {
    url,
  }: {
    url: string
  },
  options?: UseQueryOptions<Site[]>
) {
  return useQuery(
    ['sites', url],
    async () => {
      const response = await api.get<Site[]>(url)
      return response.data
    },
    {
      meta: { persist: true },
      staleTime: 5 * 60 * 1000, // 5 minutes
      select: (sites) =>
        _(sites)
          .map((site) => ({
            ...site,
            location: getLocation(site.longitude, site.latitude),
          }))
          .orderBy((site) => site.name.toLowerCase())
          .value(),
      ...options,
    }
  )
}

/**
 * legacy logic to ensure longitude and latitude are either
 * defined or undefined at the same time.
 */
export const getLocation = (
  longitude?: number | null,
  latitude?: number | null
): [number, number] | undefined =>
  longitude != null && latitude != null ? [longitude, latitude] : undefined
