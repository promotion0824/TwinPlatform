import { api } from '@willow/ui'
import { useQuery } from 'react-query'

interface PointTag {
  name?: string
  feature?: string
}

interface AssetProperty {
  displayName?: string
  value?: string
}

export interface AssetDetails {
  id: string
  twinId?: string
  name?: string
  hasLiveData: boolean
  tags?: string[]
  pointTags?: PointTag[]
  equipmentId?: string
  categoryId: string
  floorId?: string
  properties?: AssetProperty[]
  geometry?: number[]
  identifier?: string
  moduleTypeNamePath?: string
  forgeViewerModelId?: string
  floorCode?: string
  isEquipmentOnly?: boolean
}

/**
 * This service is used to fetch specific asset details based on
 * assetId and siteId
 */
export const useGetSelectedAsset = (
  {
    siteId,
    assetId,
  }: {
    siteId: string
    assetId: string
  },
  options: { enabled: boolean }
) =>
  useQuery<AssetDetails>(
    ['selectedAssetQuery'],
    async () => {
      const response = await api.get(`/sites/${siteId}/assets/${assetId}`)
      return response.data
    },
    options
  )
