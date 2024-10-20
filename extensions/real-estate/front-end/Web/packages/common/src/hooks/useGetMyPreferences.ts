import { api } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'

/**
 * This is the hook to retrieve latest preferences of the current user.
 * Unlike useGetMe, which retrieves user data with a 5-minute cache,
 * this hook ensures that the most up-to-date preferences are fetched
 * directly from the DirectoryCore.
 */
export default function useGetMyPreferences<T>(
  options?: UseQueryOptions<{
    language?: string
    profile?: {
      [key: string]: T
    }
  }>
) {
  return useQuery(
    ['my-preferences'],
    async () => {
      const response = await api.get('/me/preferences')
      return response.data
    },
    {
      ...options,
    }
  )
}
