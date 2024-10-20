import { useRef } from 'react'
import { useEventListener, useThrottle } from '@willow/ui'
import { useEditor } from './EditorContext'

export default function HandleTouchZoomEditor() {
  const pointersRef = useRef([])
  const lastMagnitudeRef = useRef()
  const editor = useEditor()

  function zoom(x, y, nextZoom) {
    if (editor.zoom === nextZoom) {
      return
    }

    const scale = nextZoom / 100 / (editor.zoom / 100)
    const nextX = (editor.x - x) * scale + x
    const nextY = (editor.y - y) * scale + y

    editor.move({
      x: nextX,
      y: nextY,
      zoom: nextZoom,
    })
  }

  function handlePointerDown(e) {
    pointersRef.current = [...pointersRef.current, e]
  }

  function handlePointerUp(e) {
    pointersRef.current = pointersRef.current.filter(
      (prevEvent) => prevEvent.pointerId !== e.pointerId
    )

    lastMagnitudeRef.current = undefined
  }

  function handlePointerMove(e) {
    pointersRef.current = pointersRef.current.map((prevEvent) =>
      prevEvent.pointerId === e.pointerId ? e : prevEvent
    )

    if (pointersRef.current.length === 2) {
      e.preventDefault()

      const vector = [
        pointersRef.current[1].clientX - pointersRef.current[0].clientX,
        pointersRef.current[1].clientY - pointersRef.current[0].clientY,
      ]
      const magnitude = Math.sqrt(vector[0] * vector[0] + vector[1] * vector[1])

      if (lastMagnitudeRef.current != null) {
        const rect = editor.contentRef.current.getBoundingClientRect()

        const minX = Math.min(
          pointersRef.current[0].clientX,
          pointersRef.current[1].clientX
        )
        const maxX = Math.max(
          pointersRef.current[0].clientX,
          pointersRef.current[1].clientX
        )
        const minY = Math.min(
          pointersRef.current[0].clientY,
          pointersRef.current[1].clientY
        )
        const maxY = Math.max(
          pointersRef.current[0].clientY,
          pointersRef.current[1].clientY
        )
        const centerX = (maxX - minX) / 2 + minX - rect.x
        const centerY = (maxY - minY) / 2 + minY - rect.y

        const delta = magnitude - lastMagnitudeRef.current

        if (delta > 5) {
          const nextZoom =
            editor.zoomLevels.find((zoomLevel) => zoomLevel > editor.zoom) ??
            editor.zoomLevels.slice(-1)[0]

          zoom(centerX, centerY, nextZoom)
        } else if (delta < -5) {
          const nextZoom =
            [...editor.zoomLevels]
              .reverse()
              .find((zoomLevel) => zoomLevel < editor.zoom) ??
            editor.zoomLevels[0]

          zoom(centerX, centerY, nextZoom)
        }
      }

      lastMagnitudeRef.current = magnitude
    }
  }

  const throttledHandlePointerMove = useThrottle(handlePointerMove, 50)

  useEventListener(editor.contentRef, 'wheel', (e) => {
    const rect = editor.contentRef.current.getBoundingClientRect()
    const x = e.clientX - rect.x
    const y = e.clientY - rect.y

    if (e.deltaY < 0) {
      const nextZoom =
        editor.zoomLevels.find((zoomLevel) => zoomLevel > editor.zoom) ??
        editor.zoomLevels.slice(-1)[0]

      zoom(x, y, nextZoom)
    } else {
      const nextZoom =
        [...editor.zoomLevels]
          .reverse()
          .find((zoomLevel) => zoomLevel < editor.zoom) ?? editor.zoomLevels[0]

      zoom(x, y, nextZoom)
    }
  })

  useEventListener(editor.contentRef, 'pointerdown', handlePointerDown)
  useEventListener(editor.contentRef, 'pointerup', handlePointerUp)
  useEventListener(editor.contentRef, 'pointercancel', handlePointerUp)
  useEventListener(editor.contentRef, 'pointerout', handlePointerUp)
  useEventListener(editor.contentRef, 'pointerleave', handlePointerUp)
  useEventListener(editor.contentRef, 'pointermove', throttledHandlePointerMove)

  return null
}
