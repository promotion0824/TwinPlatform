import { useRef, useState } from 'react'
import { useEventListener, useWindowEventListener, Blocker } from '@willow/ui'
import { useEditor } from './EditorContext'
import { useFloor } from '../FloorContext'
import styles from './HandlePanEditor.css'

export default function HandlePanEditor() {
  const floor = useFloor()
  const editor = useEditor()

  const pointersRef = useRef([])
  const startRef = useRef()
  const [isDragging, setIsDragging] = useState(false)

  function handlePointerDown(e) {
    pointersRef.current = [...pointersRef.current, e]

    const RIGHT_BUTTON = 2
    const shouldMove = e.button === RIGHT_BUTTON || floor.mode === 'view'
    if (shouldMove) {
      startRef.current = {
        x: editor.x,
        y: editor.y,
        clientX: e.clientX,
        clientY: e.clientY,
      }
    }
  }

  function handlePointerUp() {
    pointersRef.current = []
    startRef.current = undefined
    setIsDragging(false)
  }

  function handlePointerMove(e) {
    if (pointersRef.current.length === 1 && startRef.current != null) {
      if (pointersRef.current[0].pointerType === 'mouse') {
        setIsDragging(true)
      }

      const diffX = e.clientX - startRef.current.clientX
      const diffY = e.clientY - startRef.current.clientY

      editor.move({
        x: startRef.current.x + diffX,
        y: startRef.current.y + diffY,
      })
    }
  }

  useEventListener(editor.contentRef, 'pointerdown', handlePointerDown)
  useWindowEventListener('pointerup', handlePointerUp)
  useWindowEventListener('pointermove', handlePointerMove)

  if (!isDragging) {
    return null
  }

  return <Blocker className={styles.blocker} />
}
