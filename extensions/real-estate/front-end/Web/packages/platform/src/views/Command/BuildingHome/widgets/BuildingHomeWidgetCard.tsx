import { forwardRef } from 'react'

import { throwErrorInDevelopmentMode } from '@willow/common'
import WidgetCard, {
  WidgetCardProps,
} from '../../../../components/LocationHome/WidgetCard/WidgetCard'
import {
  isWidgetId,
  useBuildingHomeSlice,
  WidgetId,
} from '../../../../store/buildingHomeSlice'

interface BuildingHomeWidgetCardProps extends WidgetCardProps {
  /** Widget id, which will be used for deleting the Widget */
  id: WidgetId
}

/**
 * The `WidgetCard` component that has the `onWidgetDelete` function default bound
 * to `removeWidget` from `useBuildingHomeWidgetsContext`
 */
const BuildingHomeWidgetCard = forwardRef<
  HTMLDivElement,
  BuildingHomeWidgetCardProps
>(({ id, ...props }, ref) => {
  const { removeWidget } = useBuildingHomeSlice()

  if (!isWidgetId(id)) {
    throwErrorInDevelopmentMode(
      `Please provide a valid widgetId to delete the widget. Current id is: ${id}`
    )
  }

  return (
    <WidgetCard
      ref={ref}
      onWidgetDelete={() => {
        removeWidget(id)
      }}
      {...props}
    />
  )
})

export default BuildingHomeWidgetCard
