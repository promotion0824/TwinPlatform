import { useEditor } from './EditorContext'

export default function useZoomEditor() {
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

  return {
    zoomAtCenter(nextZoom) {
      const x = editor.contentRef.current.offsetWidth / 2
      const y = editor.contentRef.current.offsetHeight / 2

      zoom(x, y, nextZoom)
    },
  }
}
