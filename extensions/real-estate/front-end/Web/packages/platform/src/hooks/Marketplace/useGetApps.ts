import { api } from '@willow/ui'
import { AxiosError } from 'axios'
import { useQuery } from 'react-query'
import { MarketplaceApp } from '../../views/Marketplace/types'

export default function useGetApps(siteId: string) {
  return useQuery<MarketplaceApp[], AxiosError>(['apps', siteId], async () => {
    const response = await api.get('/apps', { params: { siteId } })
    return response.data
  })
}
