import { useRef, useEffect } from 'react'
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { extend, useFrame, useThree } from '@react-three/fiber'
import { useDebounce, useTimer } from '@willow/ui'

extend({ OrbitControls })

export default function Controls({ floorsContext }) {
  const { gl, camera } = useThree()
  const timer = useTimer()

  const controls = useRef()

  const debouncedOnControlsChange = useDebounce(() => {
    if (!floorsContext.hasCameraMoved) {
      floorsContext.setHasCameraMoved(true)
    }
  }, 50)

  useFrame(() => {
    controls.current.update()
  })

  const handleControlsChange = () => {
    if (!floorsContext.resetCamera) {
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
    async function update() {
      if (floorsContext.resetCamera) {
        controls.current?.reset()

        floorsContext.setResetCamera(false)

        await timer.sleep(80)

        floorsContext.setResetCamera(false)
        floorsContext.setHasCameraMoved(false)
      }
    }

    update()
  }, [floorsContext.resetCamera])

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
