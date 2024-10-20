import { useAnalytics } from '@willow/ui'
import { useFloor } from '../../../../../FloorContext'
import Layer from './Layer/Layer'

export default function ImageLayer({ image, onDeleteClick }) {
  const analytics = useAnalytics()
  const floor = useFloor()

  const handleClick = () => {
    analytics.track(
      image.isVisible ? '2D Layer Deselected' : '2D Layer Selected',
      {
        layer_name: image.typeName,
      }
    )
    floor.toggleImage(image.id)
  }

  return (
    <Layer
      header={image.typeName}
      isVisible={image.isVisible}
      onVisibilityClick={handleClick}
      onDeleteClick={
        !floor.isReadOnly && image.canBeDeleted ? onDeleteClick : undefined
      }
    />
  )
}
