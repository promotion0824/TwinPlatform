import { api } from '@willow/ui'
import { useQuery } from 'react-query'
import { MarketplaceApp } from '../../views/Marketplace/types'

export default function useGetSiteApp(siteId: string, appId: string) {
  return useQuery<MarketplaceApp>(['apps', siteId, appId], async () => {
    const response = await api.get(`/apps/${appId}`, { params: { siteId } })
    return response.data
  })
}
