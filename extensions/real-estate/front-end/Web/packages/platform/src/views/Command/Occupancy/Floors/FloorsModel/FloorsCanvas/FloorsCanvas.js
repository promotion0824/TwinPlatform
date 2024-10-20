import { useEffect, useMemo } from 'react'
import { useHistory, useParams } from 'react-router'
import { Canvas } from '@react-three/fiber'
import { useAnalytics, useUserAgent } from '@willow/ui'
import { useFloors } from '../../FloorsContext'
import Controls from './Controls'
import Floor from './Floor/Floor'

function toRadians(angle) {
  return (angle * Math.PI) / 180
}

export default function Floors({ floors }) {
  const analytics = useAnalytics()
  const floorsContext = useFloors()
  const history = useHistory()
  const params = useParams()
  const userAgent = useUserAgent()

  const max = 100 * floors.length

  const cameraPosition = useMemo(() => {
    let radius = 180
    if (floors.length > 10) radius = 300
    if (floors.length > 30) radius = 500
    if (floors.length > 60) radius = 700

    const yaw = toRadians(-30)
    const pitch = toRadians(-30)

    return [
      Math.sin(yaw) * Math.cos(pitch) * radius,
      Math.sin(yaw) * Math.sin(pitch) * radius,
      Math.cos(yaw) * radius,
    ]
  }, [floors.length])

  useEffect(
    () => () => {
      document.body.style.cursor = ''
    },
    []
  )

  return (
    <Canvas
      camera={{
        position: cameraPosition,
        fov: 60,
      }}
    >
      <ambientLight />
      <pointLight position={[max, max, max]} intensity={0.4} />
      <mesh
        position={[0, (floors.length * 10) / 2, 0]}
        rotation={[Math.PI / 2, 0, 0]}
      >
        {floors.map((floor, i) => (
          <Floor
            key={floor.id}
            floor={floor}
            position={[0, 0, i * 10]}
            isHovering={
              floorsContext.hoverFloorId === floor.id ||
              floorsContext.isHoverFloorWholeSite
            }
            onClick={(e) => {
              e.stopPropagation()
              analytics.track('Floor select from building')
              history.push(
                `/sites/${params.siteId}/occupancy/floors/${floor.id}`
              )
            }}
            onPointerDown={() => {
              if (userAgent.isIpad) {
                history.push(
                  `/sites/${params.siteId}/occupancy/floors/${floor.id}`
                )
              }
            }}
            onPointerOver={(e) => {
              e.stopPropagation()

              document.body.style.cursor = 'pointer'

              floorsContext.setHoverFloor(floor)
            }}
            onPointerMove={(e) => {
              e.stopPropagation()

              document.body.style.cursor = 'pointer'

              floorsContext.setHoverFloor(floor)
            }}
            onPointerOut={() => {
              floorsContext.setHoverFloorId((prevHoverFloorId) => {
                if (prevHoverFloorId === floor.id) {
                  document.body.style.cursor = ''
                  return undefined
                }

                return prevHoverFloorId
              })
            }}
          />
        ))}
      </mesh>
      <Controls floorsContext={floorsContext} />
    </Canvas>
  )
}
