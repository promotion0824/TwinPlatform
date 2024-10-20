import { useState } from 'react'
import OccupancyFloor from './OccupancyFloor/OccupancyFloor'
import { FloorsContext } from './FloorsContext'

export default function OccupancyFloorComponent() {
  const [hoverFloorId, setHoverFloorId] = useState()
  const [isHoverFloorWholeSite, setIsHoverFloorWholeSite] = useState(false)
  const [hasCameraMoved, setHasCameraMoved] = useState(false)
  const [resetCamera, setResetCamera] = useState(false)

  const context = {
    hoverFloorId,
    isHoverFloorWholeSite,
    hasCameraMoved,
    resetCamera,
    isReadOnly: true,

    setHoverFloor(floor) {
      setHoverFloorId(floor?.id)
      setIsHoverFloorWholeSite(
        floor?.name === 'BLDG' || floor?.name === 'SOFI CAMPUS OVERALL'
      )
    },

    setHoverFloorId,
    setHasCameraMoved,
    setResetCamera,
  }

  return (
    <FloorsContext.Provider value={context}>
      <OccupancyFloor />
    </FloorsContext.Provider>
  )
}
