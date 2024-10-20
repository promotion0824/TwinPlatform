import { extend } from '@react-three/fiber'
import { MeshLine, MeshLineMaterial } from 'threejs-meshline'

extend({ MeshLine, MeshLineMaterial })

export default function Line({ vertices, material }) {
  return (
    <mesh material={material}>
      <meshLine attach="geometry" vertices={vertices} />
    </mesh>
  )
}
