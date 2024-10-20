import { useEditMode } from './EditModeContext'
import Equipment from '../Equipment/Equipment'

export default function EditEquipment({ equipment }) {
  const editMode = useEditMode()

  return (
    <Equipment
      equipment={equipment}
      clickable
      selected={editMode.selectedObject === equipment}
      onPointerDown={(e) => {
        const LEFT_BUTTON = 0
        if (e.button === LEFT_BUTTON) {
          e.stopPropagation()
          editMode.selectObject(equipment)
        }
      }}
      onClick={(e) => e.stopPropagation()}
    />
  )
}
