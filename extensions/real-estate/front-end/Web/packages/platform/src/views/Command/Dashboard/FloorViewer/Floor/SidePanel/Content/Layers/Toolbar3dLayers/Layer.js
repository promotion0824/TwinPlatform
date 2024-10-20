import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import { useAnalytics, Button } from '@willow/ui'
import { Group } from '@willowinc/ui'
import { useFloor } from '../../../../FloorContext'
import styles from './Layer.css'

export default function Layer({
  layer,
  isSelected,
  iframeWindow,
  onClick,
  onDeselect,
  onDeleteClick,
  isInsideGroup,
}) {
  const analytics = useAnalytics()
  const floor = useFloor()
  const isLayerAssetSelected = floor.selectedAsset?.moduleTypeNamePath
    ? floor.selectedAsset?.moduleTypeNamePath.some(
        (namePath) => namePath.toLowerCase() === layer.typeName.toLowerCase()
      )
    : false

  /**
   * Classic Viewer (3D Viewer) will require a layer (a 3d file containing many 3d objects)
   * to be loaded to show a 3D representation of an selected asset. If layer.url is undefined,
   * we would not be able to select/show it on 3D viewer, so we deselect the asset and not focus on it.
   * Otherwise, when 3D Viewer is mounted and its "loadModel" function is ready,
   * we load it into 3D View focusing on the asset
   */
  useEffectOnceMounted(() => {
    if (isSelected) {
      if (iframeWindow?.loadModel != null && layer.url != null) {
        iframeWindow?.loadModel(layer.url, layer.typeName)
      } else if (layer.url == null) {
        onDeselect()
      }
    } else {
      iframeWindow?.hideModel?.(layer.url)
    }
  }, [isSelected, layer, iframeWindow?.loadModel])

  const handleClick = () => {
    analytics.track(isSelected ? '3D Model Deselected' : '3D Model Selected', {
      system_name: layer.typeName,
    })
    onClick(!isSelected)
  }

  const cxClassName = cx(styles.layer, {
    [styles.insideGroup]: isInsideGroup,
    [styles.selected]: isSelected,
    [styles.assetSelected]: isLayerAssetSelected,
  })

  return (
    <Group
      className={cxClassName}
      onClick={handleClick}
      css={{
        flexFlow: 'row',
        cursor: 'pointer',
      }}
    >
      <Button
        icon={isSelected ? 'eye-open' : 'eye-close'}
        iconSize="small"
        className={styles.eye}
      />
      <input value={layer.typeName} readOnly className={styles.input} />
      {!floor.isReadOnly && layer.canBeDeleted && (
        <Button icon="close" iconSize="small" onClick={onDeleteClick} />
      )}
    </Group>
  )
}
