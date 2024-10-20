import { useEffectOnceMounted } from '@willow/common'
import { useFloor } from '../../FloorContext'
import { useEditor } from '../EditorContext'

export default function CenterWhenSelected({ targetRef, selected = true }) {
  const editor = useEditor()
  const floor = useFloor()

  useEffectOnceMounted(() => {
    if (selected) {
      const sideWidth =
        document.getElementById('side-panel-header')?.offsetWidth < 40 ? 633 : 0

      const rect = targetRef.current.getBoundingClientRect()
      const contentRect = editor.contentRef.current.getBoundingClientRect()
      const svgRect = editor.svgRef.current.getBoundingClientRect()

      const x = svgRect.x - rect.x + (contentRect.width - sideWidth) / 2
      const y = svgRect.y - rect.y + contentRect.height / 2

      editor.move({ x, y })
    }
  }, [floor.centeredEquipmentId])

  return null
}
