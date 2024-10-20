import { useState } from 'react'
import { useEventListener, useWindowEventListener } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import { useEditor } from '../EditorContext'
import { useEditMode } from './EditModeContext'
import Zone from '../Zone/Zone'
import styles from './EditZone.css'

export default function CopiedZone() {
  const floor = useFloor()
  const editor = useEditor()
  const editMode = useEditMode()

  const [mouse, setMouse] = useState()
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

    x = +x.toFixed(0)
    y = +y.toFixed(0)

    if (isShiftDown) {
      setMouse(editor.getClosestPoint([x, y]))
    } else {
      setMouse([x, y])
    }
  })

  const minX = Math.min(...editMode.copiedZone.points.map((point) => point[0]))
  const minY = Math.min(...editMode.copiedZone.points.map((point) => point[1]))

  const zone =
    mouse != null
      ? {
          ...editMode.copiedZone,
          points: editMode.copiedZone.points.map((point) => [
            point[0] - minX + mouse[0],
            point[1] - minY + mouse[1],
          ]),
        }
      : editMode.copiedZone

  useEventListener(editor.svgRef, 'click', () => {
    floor.addZone(zone)
  })

  useWindowEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      editMode.setCopiedZone()
    }
  })

  return <Zone zone={zone} className={styles.zone} />
}
