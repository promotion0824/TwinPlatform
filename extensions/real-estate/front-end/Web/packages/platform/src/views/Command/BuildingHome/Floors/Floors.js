import { ErrorBoundary } from '@willow/ui'
import { useMemo } from 'react'
import Floors from './Floors/Floors'
import getFloors from './getFloors'

export default function FloorsComponent({ floors }) {
  const nextFloors = useMemo(() => getFloors(floors), [floors])

  return (
    <ErrorBoundary>
      <div
        css={{
          position: 'relative',
          width: '100%',
          height: '100%',
        }}
      >
        <Floors floors={nextFloors} />
      </div>
    </ErrorBoundary>
  )
}
