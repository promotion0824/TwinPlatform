import { api } from '@willow/ui'
import { AxiosError } from 'axios'
import { useQuery } from 'react-query'

type AppCategory = {
  id: string
  name: string
}

export default function useGetAppCategories() {
  return useQuery<AppCategory[], AxiosError>(['appCategories'], async () => {
    const response = await api.get('/appCategories')
    return response.data
  })
}
