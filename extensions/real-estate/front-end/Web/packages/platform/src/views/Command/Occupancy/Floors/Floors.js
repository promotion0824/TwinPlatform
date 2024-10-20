import { useState } from 'react'
import { Flex } from '@willow/ui'
import FloorsModel from './FloorsModel/FloorsModel'
import FloorSelector from './FloorSelector/FloorSelector'
import { FloorsContext } from './FloorsContext'

export default function FloorsComponent({ floors }) {
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
      <Flex horizontal fill="content hidden" size="small" overflow="hidden">
        <FloorSelector floors={floors} />
        <FloorsModel floors={floors} />
      </Flex>
    </FloorsContext.Provider>
  )
}
