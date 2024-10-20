import { useLayoutEffect } from 'react'
import { useFloor } from '../FloorContext'
import { useEditor } from './EditorContext'

export default function useInitializeEditor() {
  const editor = useEditor()
  const floor = useFloor()

  function centerImage() {
    const maxWidth = Math.max(editor.contentRef.current.offsetWidth, 100)
    const maxHeight = Math.max(editor.contentRef.current.offsetHeight - 30, 100)

    // eslint-disable-next-line
    const zoomLevels = [
      10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44,
      46, 48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70, 72, 74, 76, 78, 80,
      82, 84, 86, 88, 90, 92, 94, 96, 98, 100, 110, 120, 130, 140, 150, 160,
      170, 180, 190, 200, 220, 240, 260, 280, 300, 350, 400, 450, 500, 550, 600,
      650, 700, 750, 800, 900, 1000, 1100, 1200, 1400, 1600, 1800, 2000,
    ]

    const nextZoom =
      [...zoomLevels].reverse().find((zoomLevel) => {
        const scaledWidth = (floor.width * zoomLevel) / 100
        const scaledHeight = (floor.height * zoomLevel) / 100

        return scaledWidth < maxWidth && scaledHeight < maxHeight
      }) ?? zoomLevels[0]

    const nextZoomIndex = zoomLevels.findIndex(
      (zoomLevel) => zoomLevel === nextZoom
    )

    const nextX = maxWidth / 2 - (floor.width * nextZoom) / 200
    const nextY = maxHeight / 2 - (floor.height * nextZoom) / 200

    editor.setZoomLevels(zoomLevels.slice(nextZoomIndex))
    editor.move({
      x: nextX,
      y: nextY,
      zoom: nextZoom,
    })
  }

  useLayoutEffect(() => {
    centerImage()
  }, [])
}
