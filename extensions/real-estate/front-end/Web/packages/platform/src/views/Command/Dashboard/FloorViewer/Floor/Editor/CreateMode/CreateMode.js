import { useState } from 'react'
import { useEventListener, useWindowEventListener } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import { useEditor } from '../EditorContext'
import DropEquipment from '../DropEquipment/DropEquipment'
import CreateZone from './CreateZone'
import Equipment from '../Equipment/Equipment'
import Zone from '../Zone/Zone'

export default function CreateMode() {
  const floor = useFloor()
  const editor = useEditor()

  const [mouse, setMouse] = useState()
  const [zone, setZone] = useState()
  const [isShiftDown, setIsShiftDown] = useState(false)

  useWindowEventListener('keydown', (e) => {
    if (e.key === 'Shift') {
      if (mouse != null) {
        setMouse(editor.getClosestPoint(mouse))
      }
      setIsShiftDown(true)
    }
  })

  useWindowEventListener('keyup', (e) => {
    if (e.key === 'Shift') {
      setIsShiftDown(false)
    }
  })

  useEventListener(editor.svgRef, 'pointermove', (e) => {
    const scale = 100 / editor.zoom

    const rect = e.currentTarget.getBoundingClientRect()
    let x = (e.clientX - rect.left) * scale
    let y = (e.clientY - rect.top) * scale

    // snap mouse to starting point if mouse is close
    if (zone != null && mouse != null) {
      const firstPoint = zone.points[0]
      if (Math.abs(firstPoint[0] - x) < 5 && Math.abs(firstPoint[1] - y) < 5) {
        ;[x, y] = firstPoint
      }
    }

    x = +x.toFixed(0)
    y = +y.toFixed(0)

    if (isShiftDown) {
      setMouse(editor.getClosestPoint([x, y]))
    } else {
      setMouse([x, y])
    }
  })

  useEventListener(editor.svgRef, 'pointerleave', () => {
    setMouse()
  })

  let layerGroups = []
  if (floor.layerGroup != null) {
    if (floor.layerGroup.name === 'Assets layer') {
      layerGroups = floor.layerGroups
    } else {
      layerGroups = [floor.layerGroup]
    }
  }
  const zones = floor.layerGroup?.zones ?? []
  const equipments = layerGroups.flatMap((layerGroup) => layerGroup.equipments)

  return (
    <>
      {zones.map((floorZone) => (
        <Zone key={floorZone.localId} zone={floorZone} />
      ))}
      {equipments.map((equipment) => (
        <Equipment key={equipment.localId} equipment={equipment} />
      ))}
      <CreateZone mouse={mouse} zone={zone} setZone={setZone} />
      <DropEquipment />
    </>
  )
}
