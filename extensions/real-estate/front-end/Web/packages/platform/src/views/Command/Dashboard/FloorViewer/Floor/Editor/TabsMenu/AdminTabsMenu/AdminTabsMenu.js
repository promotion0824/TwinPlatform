import cx from 'classnames'
import { Button, Flex, Icon, TabsHeader } from '@willow/ui'
import { useFloor } from '../../../FloorContext'
import { useEditMode } from '../../EditMode/EditModeContext'
import TabsButton from './AdminTabsButton'
import EditorTypeButtons from '../EditorTypeButtons'
import styles from './AdminTabsMenu.css'

export default function TabsMenu() {
  const floor = useFloor()
  const editMode = useEditMode()

  const showCopyButton =
    floor.mode === 'edit' &&
    ((editMode.selectedObject?.points.length > 1 &&
      editMode.selectedPointIndex == null) ||
      editMode.copiedZone != null)
  const showTrashButton =
    floor.mode === 'edit' &&
    editMode.selectedObject != null &&
    editMode.selectedPointIndex == null
  const isEquipment = editMode.selectedObject?.pointTags != null

  return (
    <TabsHeader>
      <Flex horizontal fill="header hidden" height="100%">
        {floor.floorViewType === '2D' && (
          <Flex
            horizontal
            align="center middle"
            width="100%"
            className={styles.buttons}
          >
            {floor.layerGroup?.name !== 'Assets layer' && (
              <TabsButton
                icon="create"
                mode="create"
                data-tooltip="Draw zone"
              />
            )}
            <TabsButton
              icon="edit"
              mode="edit"
              data-tooltip="Edit zone/equipment"
            />
            <Flex horizontal align="middle" className={styles.tools}>
              <Button
                icon="copy"
                iconSize="small"
                className={cx(styles.button, {
                  [styles.visible]: showCopyButton,
                })}
                data-tooltip="Copy zone"
                onClick={() => editMode.setCopiedZone(editMode.selectedObject)}
              />
              <Button
                icon="trash"
                iconSize="small"
                className={cx(styles.button, {
                  [styles.visible]: showTrashButton,
                })}
                data-tooltip={`Delete ${isEquipment ? 'equipment' : 'zone'}`}
                onClick={() => floor.deleteObject(editMode.selectedObject)}
              />
            </Flex>
          </Flex>
        )}
        <Flex horizontal size="medium" align="right middle" padding="0 medium">
          {floor.lastSavedTime === 'loading' && (
            <Icon icon="progress" size="small" />
          )}
          <EditorTypeButtons />
        </Flex>
      </Flex>
    </TabsHeader>
  )
}
