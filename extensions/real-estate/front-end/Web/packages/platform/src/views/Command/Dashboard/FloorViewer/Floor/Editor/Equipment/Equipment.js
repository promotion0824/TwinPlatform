import { useRef } from 'react'
import cx from 'classnames'
import { Flex, Icon, Text } from '@willow/ui'
import { useFloor } from '../../FloorContext'
import CenterWhenSelected from '../CenterWhenSelected/CenterWhenSelected'
import EquipmentTooltip from './EquipmentTooltip'
import SelectZoneLayer from './SelectZoneLayer'
import Tooltip from '../Tooltip/Tooltip'
import styles from './Equipment.css'

export default function Equipment({
  type = 'normal',
  equipment,
  clickable,
  selected,
  className,
  onPointerDown,
  onClick,
  ...rest
}) {
  const floor = useFloor()
  const equipmentRef = useRef()

  let priority
  if (equipment.priority === 1) priority = 'red'
  if (equipment.priority === 2) priority = 'orange'
  if (equipment.priority === 3) priority = 'yellow'

  const showDetails = type === 'details'
  const assetsLayer = floor.layerGroups.find(
    (layerGroup) => layerGroup.name === 'Assets layer'
  )
  const isAssetsLayerEquipment = assetsLayer?.equipments.some(
    (layerGroupEquipment) => layerGroupEquipment.id === equipment.id
  )

  const cxClassName = cx(
    styles.equipment,
    {
      [styles.isClickable]: clickable,
      [styles.isSelected]: selected,
      [styles.colorRed]: priority === 'red',
      [styles.colorOrange]: priority === 'orange',
      [styles.colorYellow]: priority === 'yellow',
    },
    className
  )
  const cxTooltipClassName = cx(styles.tooltip, {
    [styles.isContentSelected]: selected,
    [styles.isSelected]: selected,
    [styles.showDetails]: showDetails,
    [styles.colorRed]: priority === 'red',
    [styles.colorOrange]: priority === 'orange',
    [styles.colorYellow]: priority === 'yellow',
  })

  return (
    <>
      <circle
        ref={equipmentRef}
        cx={equipment.points[0][0]}
        cy={equipment.points[0][1]}
        r={2}
        {...rest}
        className={cxClassName}
        onPointerDown={onPointerDown}
        onClick={onClick}
      />
      <CenterWhenSelected targetRef={equipmentRef} selected={selected} />
      {type === 'details' && (
        <EquipmentTooltip
          equipmentRef={equipmentRef}
          equipment={equipment}
          selected={selected}
          priority={priority}
          onPointerDown={onPointerDown}
          onClick={onClick}
        />
      )}
      {type !== 'details' && (
        <Tooltip
          targetRef={equipmentRef}
          point={equipment.points}
          clickable={clickable}
          selected={selected}
          className={cxTooltipClassName}
          onPointerDown={onPointerDown}
          onClick={onClick}
        >
          <Flex
            horizontal
            align="middle"
            size="small"
            padding="medium"
            className={styles.header}
          >
            {isAssetsLayerEquipment && (
              <Icon
                icon="error"
                color="white"
                data-tooltip="No specific zone layer selected"
              />
            )}
            <Text>{equipment.name}</Text>
          </Flex>
          {!showDetails && selected && (
            <SelectZoneLayer equipment={equipment} />
          )}
        </Tooltip>
      )}
    </>
  )
}
