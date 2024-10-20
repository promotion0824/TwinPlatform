import { useEffect } from 'react'
import { useAutodeskViewer } from '../../AutodeskViewerContext'
import colors from './colors'

export default function ColorAsset({
  node,
  model,
  response,
  pinOnLayerResponse,
}) {
  const autodeskViewer = useAutodeskViewer()

  useEffect(() => {
    const peopleCount =
      pinOnLayerResponse.liveDataPoints?.find(
        (liveDataPoint) => liveDataPoint.tag === 'People Count Sensor'
      )?.liveDataValue ?? 0

    // as per https://dev.azure.com/willowdev/Unified/_workitems/edit/71839
    // calculation of seatingCapacity will be updated from using property of "seatingCapacity"
    // to "person capacity", we keep the legacy calculation for backward compatibility
    const seatingCapacity =
      response.properties?.find(
        (property) => property?.displayName?.toLowerCase() === 'person capacity'
      )?.value ??
      response.properties?.find(
        (property) => property.displayName === 'Capacity'
      )?.value?.seatingCapacity ??
      0

    let color = colors.green
    if (peopleCount > 0) {
      color = colors.yellow
    }
    if (peopleCount > 0 && peopleCount === seatingCapacity) {
      color = colors.orange
    }
    if (peopleCount > 0 && peopleCount > seatingCapacity) {
      color = colors.red
    }

    autodeskViewer.viewer.setThemingColor(node.dbId, color, model.model, true)

    return () => {
      autodeskViewer.viewer.setThemingColor(node.dbId, null, model.model, true)
    }
  }, [])

  return null
}
