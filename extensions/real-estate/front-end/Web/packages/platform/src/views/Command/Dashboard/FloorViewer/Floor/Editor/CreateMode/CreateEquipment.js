import { useEventListener } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import { useEditor } from '../EditorContext'

export default function CreateEquipment() {
  const floor = useFloor()
  const editor = useEditor()

  useEventListener(editor.svgRef, 'dragover', (e) => {
    e.preventDefault()
  })

  useEventListener(editor.svgRef, 'drop', (e) => {
    const data = JSON.parse(e.dataTransfer.getData('text/plain'))

    const rect = e.currentTarget.getBoundingClientRect()
    const scale = 100 / editor.zoom
    let x = (e.clientX - rect.x) * scale
    let y = (e.clientY - rect.y) * scale
    x = +x.toFixed(0)
    y = +y.toFixed(0)

    floor.addEquipment({
      ...data,
      points: [[x, y]],
    })
  })

  return null
}
