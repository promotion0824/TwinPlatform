import { useParams } from 'react-router'
import { Fetch, Spacing } from '@willow/mobile-ui'
import { useLayout } from 'providers'
import FloorSelector from './FloorSelector/FloorSelector'
import AssetSelector from './AssetSelector/AssetSelector'

export default function Floor() {
  const params = useParams()
  const { setShowBackButton } = useLayout()

  setShowBackButton(false)

  return (
    <Fetch url={`/api/sites/${params.siteId}/floors`}>
      {(floors) => (
        <Spacing vertical type="content" padding="medium">
          <FloorSelector floors={floors} />
          <AssetSelector />
        </Spacing>
      )}
    </Fetch>
  )
}
