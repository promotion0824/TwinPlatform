import { Flex, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../FloorContext'
import styles from './Equipment.css'

export default function SelectZoneLayer({ equipment }) {
  const floor = useFloor()
  const { t } = useTranslation()

  const equipmentLayerGroup = floor.layerGroups
    .filter(
      (layerGroup) =>
        layerGroup.name !== 'Assets layer' && layerGroup.name !== 'Floor layer'
    )
    .find((layerGroup) =>
      layerGroup.equipments.some(
        (layerGroupEquipment) => layerGroupEquipment.id === equipment.id
      )
    )

  const layerGroups = floor.layerGroups.filter(
    (layerGroup) =>
      layerGroup.name !== 'Assets layer' && layerGroup.name !== 'Floor layer'
  )

  function handleChange(nextLayerGroupId) {
    floor.setLayerGroupId(equipment, nextLayerGroupId)
  }

  return (
    <Flex size="medium" padding="medium" className={styles.content}>
      <Select
        placeholder={t('placeholder.selectZoneLayer')}
        className={styles.select}
        value={equipmentLayerGroup?.id}
        onChange={handleChange}
        onPointerDown={(e) => e.stopPropagation()}
      >
        {layerGroups.map((layerGroup) => (
          <Option
            key={layerGroup.id}
            value={layerGroup.id}
            onPointerDown={(e) => e.stopPropagation()}
          >
            {layerGroup.name}
          </Option>
        ))}
      </Select>
    </Flex>
  )
}
