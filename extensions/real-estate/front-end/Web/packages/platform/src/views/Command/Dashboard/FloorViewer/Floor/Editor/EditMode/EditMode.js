import { useWindowEventListener } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import DropEquipment from '../DropEquipment/DropEquipment'
import CopiedZone from './CopiedZone'
import EditEquipment from './EditEquipment'
import EditZone from './EditZone'
import { useEditMode } from './EditModeContext'

export default function EditMode() {
  const editMode = useEditMode()
  const floor = useFloor()

  useWindowEventListener('click', (e) => {
    const LEFT_BUTTON = 0
    if (e.button === LEFT_BUTTON) {
      editMode.selectObject()
    }
  })

  useWindowEventListener('keydown', (e) => {
    if (editMode.selectedObject != null) {
      if (e.key === 'ArrowLeft') {
        editMode.moveObject(editMode.selectedObject, -1, 0)
      }
      if (e.key === 'ArrowRight') {
        editMode.moveObject(editMode.selectedObject, 1, 0)
      }
      if (e.key === 'ArrowUp') {
        editMode.moveObject(editMode.selectedObject, 0, -1)
      }
      if (e.key === 'ArrowDown') {
        editMode.moveObject(editMode.selectedObject, 0, 1)
      }
    }
  })

  return (
    <>
      {editMode.zones.map((zone) => (
        <EditZone key={zone.localId} zone={zone} />
      ))}
      {editMode.equipments.map((equipment) => (
        <EditEquipment key={equipment.localId} equipment={equipment} />
      ))}
      {editMode.copiedZone != null && <CopiedZone />}
      {!floor.isReadOnly && <DropEquipment />}
    </>
  )
}
