import { useRef, useEffect } from 'react'
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { extend, useFrame, useThree } from '@react-three/fiber'
import { useDebounce } from '@willow/ui'

extend({ OrbitControls })

export default function Controls({
  resetCamera,
  onFinishResetCamera,
  onControlsChange = () => {},
}) {
  const { gl, camera } = useThree()
  const controls = useRef()
  const debouncedOnControlsChange = useDebounce(onControlsChange, 50)

  useFrame(() => {
    controls.current.update()
  })

  const handleControlsChange = () => {
    if (!resetCamera) {
      debouncedOnControlsChange()
    }
  }

  useEffect(() => {
    controls.current?.addEventListener('change', handleControlsChange)

    return () => {
      controls.current?.removeEventListener('change', handleControlsChange)
    }
  }, [])

  useEffect(() => {
    if (resetCamera) {
      controls.current?.reset()
      onFinishResetCamera()
    }
  }, [resetCamera])

  return (
    <orbitControls
      ref={controls}
      args={[camera, gl.domElement]}
      screenSpacePanning
      enableDamping
      dampingFactor={0.2}
      minDistance={100}
      maxDistance={800}
      mouseButtons={{
        LEFT: THREE.MOUSE.PAN,
        RIGHT: THREE.MOUSE.ROTATE,
      }}
    />
  )
}
