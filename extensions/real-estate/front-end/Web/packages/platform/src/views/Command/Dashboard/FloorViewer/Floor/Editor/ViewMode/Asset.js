import { useRef } from 'react'
import { Flex, Text } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import CenterWhenSelected from '../CenterWhenSelected/CenterWhenSelected'
import HideZoneLayersWhenAssetSelected from './HideZoneLayersWhenAssetSelected'
import Tooltip from '../Tooltip/Tooltip'
import styles from './Asset.css'

export default function Asset({ asset }) {
  const floor = useFloor()
  const assetRef = useRef()

  function handlePointerDown(e) {
    const LEFT_BUTTON = 0
    if (e.button === LEFT_BUTTON && floor.isReadOnly) {
      floor.selectAsset(asset)
    }
  }

  return (
    <>
      <circle
        ref={assetRef}
        cx={asset.geometry[0]}
        cy={asset.geometry[1]}
        r={2}
        className={styles.asset}
        onPointerDown={handlePointerDown}
      />
      <CenterWhenSelected targetRef={assetRef} />
      <HideZoneLayersWhenAssetSelected asset={asset} />
      <Tooltip
        targetRef={assetRef}
        point={asset.geometry}
        clickable={floor.isReadOnly}
        selected={asset.id === floor.selectedAsset?.id}
        onPointerDown={handlePointerDown}
      >
        <Flex padding="medium">
          <Text type="message">{asset.identifier}</Text>
          <Text color="white">{asset.name}</Text>
        </Flex>
      </Tooltip>
    </>
  )
}
