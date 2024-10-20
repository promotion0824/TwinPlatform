import { api } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'

export default function useGetMe(options?: UseQueryOptions) {
  return useQuery(
    ['me'],
    async () => {
      const response = await api.get('/me')
      return response.data
    },
    {
      meta: { persist: true },
      staleTime: 5 * 60 * 1000, // 5 minutes
      ...options,
    }
  )
}
