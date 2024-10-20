import { useMemo } from 'react'
import * as THREE from 'three'
import { MeshLineMaterial } from 'threejs-meshline'
import FloorZone from './FloorZone'
import colors from '../colors.json'

export default function Floor({ floor, isHovering, isSelected, ...rest }) {
  let color = 'text'
  if (floor.insightsHighestPriority === 1) color = 'red'
  if (floor.insightsHighestPriority === 2) color = 'orange'
  if (floor.insightsHighestPriority === 3) color = 'yellow'
  if (isHovering || isSelected) color = `${color}Light`
  color = colors[color]

  const meshMaterial = useMemo(
    () =>
      new THREE.MeshPhongMaterial({
        color,
        opacity: isHovering || isSelected ? 1 : 0.5,
        transparent: !isHovering,
      }),
    [color, isHovering, isSelected]
  )

  const lineMaterial = useMemo(
    () =>
      new MeshLineMaterial({
        lineWidth: 0.15,
        color: colors.light,
      }),
    []
  )

  return floor.geometry.map((geometry, i) => (
    <FloorZone
      key={`${floor.id} ${i}`} // eslint-disable-line
      {...rest}
      geometry={geometry}
      meshMaterial={meshMaterial}
      lineMaterial={lineMaterial}
    />
  ))
}
