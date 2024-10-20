import cx from 'classnames'
import { Flex, Icon, Text } from '@willow/ui'
import { useFloors } from '../../FloorsContext'
import colors from '../colors.json'
import styles from './FloorInformation.css'

export default function FloorInformation({ floor }) {
  const floorsContext = useFloors()

  let color
  if (floor.insightsHighestPriority === 1) color = 'red'
  if (floor.insightsHighestPriority === 2) color = 'orange'
  if (floor.insightsHighestPriority === 3) color = 'yellow'
  color = colors[color]

  const cxFloorClassName = cx(styles.floorInformation, {
    [styles.isVisible]: floorsContext.hoverFloorId === floor.id,
    [styles.peopleOverLimit]: floor.people > floor.peopleLimit,
    [styles.isVisible]: true,
  })

  return (
    <Flex
      key={floor.id}
      size="medium"
      padding="medium"
      className={cxFloorClassName}
    >
      <Flex>
        <Text style={{ color }}>{floor.code}</Text>
        <Text>{floor.name}</Text>
      </Flex>
      <Flex className={styles.container}>
        <Flex horizontal>
          <Flex padding="small small small 0">
            <Icon icon="user" size="tiny" />
          </Flex>
          <Text type="huge">{floor.people}</Text>
        </Flex>
        <Flex>
          <Text type="small">Limit {floor.peopleLimit}</Text>
        </Flex>
      </Flex>
    </Flex>
  )
}
