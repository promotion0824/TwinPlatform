import _ from 'lodash'
import { useAnalytics } from '@willow/ui'
import { useFloor } from '../../../../../FloorContext'
import Layer from './Layer/Layer'

export default function ZoneLayer({ layerGroup }) {
  const analytics = useAnalytics()
  const floor = useFloor()

  if (floor.isReadOnly) {
    const priority = _(layerGroup.equipments)
      .map((equipment) => equipment.priority)
      .filter((equipmentPriority) => equipmentPriority > 0)
      .orderBy((equipmentPriority) => equipmentPriority)
      .value()[0]

    let priorityColor
    if (priority === 1) priorityColor = 'red'
    if (priority === 2) priorityColor = 'orange'
    if (priority === 3) priorityColor = 'yellow'

    return (
      <Layer
        header={layerGroup.name}
        isVisible={floor.isLayerGroupVisible(layerGroup)}
        priorityColor={priorityColor}
        onVisibilityClick={() => {
          const nextVisibility = !floor.isLayerGroupVisible(layerGroup)
          analytics.track(
            nextVisibility
              ? '2D Layer Group Selected'
              : '2D Layer Group Deselected',
            {
              layer_group_name: layerGroup.name,
            }
          )
          floor.toggleIsVisible(layerGroup)
        }}
      />
    )
  }

  if (layerGroup.name === 'Assets layer') {
    return (
      <Layer
        header={layerGroup.name}
        selected={floor.layerGroup === layerGroup}
        onClick={() => floor.selectLayerGroup(layerGroup)}
      />
    )
  }

  return (
    <Layer
      header={layerGroup.name}
      selected={floor.layerGroup === layerGroup}
      onClick={() => floor.selectLayerGroup(layerGroup)}
      onChange={(value) => floor.setLayerGroupName(layerGroup, value)}
      onDeleteClick={() => floor.deleteLayerGroup(layerGroup)}
    />
  )
}
