import { useEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { Blocker } from '@willow/ui'
import styles from './Drag.css'

export default function Drag({
  className,
  children,
  moveOnDown = false,
  onDown = (drag) => drag,
  onMove = () => {},
  onUp = () => {},
}) {
  const dragRef = useRef()
  const [isDragging, setIsDragging] = useState(false)

  const cxClassName = cx(styles.drag, className)

  function handlePointerMove(e) {
    e.preventDefault()

    if (dragRef.current) {
      onMove({
        ...dragRef.current,
        clientX: e.clientX,
        clientY: e.clientY,
        diffClientX: e.clientX - dragRef.current.startClientX,
        diffClientY: e.clientY - dragRef.current.startClientY,
      })
    }
  }

  function handlePointerUp() {
    onUp(dragRef.current)
    dragRef.current = null

    window.removeEventListener('pointermove', handlePointerMove)
    window.removeEventListener('pointerup', handlePointerUp)

    setIsDragging(false)
  }

  const dragContext = {
    onPointerDown(e) {
      dragRef.current = onDown({
        currentTarget: e.currentTarget,
        startClientX: e.clientX,
        startClientY: e.clientY,
        diffClientX: 0,
        diffClientY: 0,
      })

      if (dragRef.current != null) {
        window.addEventListener('pointermove', handlePointerMove)
        window.addEventListener('pointerup', handlePointerUp)

        setIsDragging(true)

        if (moveOnDown) {
          handlePointerMove(e)
        }
      }
    },
  }

  useEffect(
    () => () => {
      window.removeEventListener('pointermove', handlePointerMove)
      window.removeEventListener('pointerup', handlePointerUp)
    },
    []
  )

  return (
    <>
      {children(dragContext)}
      {isDragging && <Blocker className={cxClassName} />}
    </>
  )
}
