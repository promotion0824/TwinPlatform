import { useEffect, useRef, useState } from 'react'

export default function useDrag(options = {}) {
  const { onDown = () => {}, onMove = () => {}, onUp = () => {} } = options

  const [drag, setDrag] = useState()

  function handleMove(position) {
    const nextDrag = {
      ...drag,
      position,
      difference: {
        x: position.clientX - drag.startPosition.clientX,
        y: drag.startPosition.clientY - position.clientY,
      },
    }

    setDrag(nextDrag)
    onMove(nextDrag)
  }

  function handleMouseMove(e) {
    e.preventDefault()
    if (drag) {
      handleMove({ clientX: e.clientX, clientY: e.clientY })
    }
  }

  function handleTouchMove(e) {
    if (drag) {
      e.preventDefault()

      handleMove({
        clientX: e.touches[0].clientX,
        clientY: e.touches[0].clientY,
      })
    }
  }

  function handleMouseUp() {
    onUp(drag)

    setDrag()
  }

  const handleMouseMoveRef = useRef(handleMouseMove)
  useEffect(() => {
    handleMouseMoveRef.current = handleMouseMove
  }, [handleMouseMove])
  function handleMouseMoveEvent(e) {
    handleMouseMoveRef.current(e)
  }

  const handleTouchMoveRef = useRef(handleTouchMove)
  useEffect(() => {
    handleTouchMoveRef.current = handleTouchMove
  }, [handleTouchMove])
  function handleTouchMoveEvent(e) {
    handleTouchMoveRef.current(e)
  }

  const handleMouseUpRef = useRef(handleMouseUp)
  useEffect(() => {
    handleMouseUpRef.current = handleMouseUp
  }, [handleMouseUp])
  function handleMouseUpEvent(e) {
    handleMouseUpRef.current(e)
    window.removeEventListener('mousemove', handleMouseMoveEvent)
    window.removeEventListener('touchmove', handleTouchMoveEvent)
    window.removeEventListener('mouseup', handleMouseUpEvent)
    window.removeEventListener('touchend', handleMouseUpEvent)
  }

  function handleDown(position) {
    const nextDrag = onDown({ startPosition: position })
    if (nextDrag === null) return

    setDrag({
      ...nextDrag,
      startPosition: position,
    })

    window.addEventListener('mousemove', handleMouseMoveEvent)
    window.addEventListener('touchmove', handleTouchMoveEvent)
    window.addEventListener('mouseup', handleMouseUpEvent)
    window.addEventListener('touchend', handleMouseUpEvent)
  }

  useEffect(
    () => () => {
      window.removeEventListener('mousemove', handleMouseMoveEvent)
      window.removeEventListener('touchmove', handleTouchMoveEvent)
      window.removeEventListener('mouseup', handleMouseUpEvent)
      window.removeEventListener('touchend', handleMouseUpEvent)
    },
    []
  )

  return {
    isDragging: drag != null,

    onMouseDown(e) {
      handleDown({ clientX: e.clientX, clientY: e.clientY })
    },

    onTouchStart(e) {
      handleDown({
        clientX: e.touches[0].clientX,
        clientY: e.touches[0].clientY,
      })
    },
  }
}
