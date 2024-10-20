import { useMemo } from 'react'
import * as THREE from 'three'
import Line from './Line'

export default function FloorZone({
  geometry,
  meshMaterial,
  lineMaterial,
  ...rest
}) {
  const shape = useMemo(() => {
    const nextShape = new THREE.Shape()
    nextShape.moveTo(...geometry[0])
    geometry.slice(1).forEach((coordinate) => {
      nextShape.lineTo(...coordinate)
    })
    return nextShape
  }, [geometry])

  const extrudeGeometry = useMemo(
    () =>
      new THREE.ExtrudeGeometry(shape, {
        depth: 10,
        bevelEnabled: false,
      }),
    [shape]
  )

  const lines = useMemo(
    () => [
      geometry.map(
        (coordinate) => new THREE.Vector3(coordinate[0], coordinate[1], 0)
      ),
      geometry.map(
        (coordinate) => new THREE.Vector3(coordinate[0], coordinate[1], 10)
      ),
    ],
    [geometry]
  )

  return (
    <mesh {...rest}>
      <mesh geometry={extrudeGeometry} material={meshMaterial} />
      {lines.map((line, i) => (
        // eslint-disable-next-line
        <Line key={i} vertices={line} material={lineMaterial} />
      ))}
    </mesh>
  )
}
