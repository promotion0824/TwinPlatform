import cx from 'classnames'
import { Flex, Text } from '@willow/ui'
import { useDashboard } from '../../../Dashboard/DashboardContext'
import colors from './colors.json'
import styles from './FloorInformation.css'

export default function FloorInformation({ floor }) {
  const dashboard = useDashboard()

  let color
  if (floor.insightsHighestPriority === 1) color = 'red'
  if (floor.insightsHighestPriority === 2) color = 'orange'
  if (floor.insightsHighestPriority === 3) color = 'yellow'
  color = colors[color]

  const cxFloorClassName = cx(styles.floorInformation, {
    [styles.isVisible]: dashboard.hoverFloorId === floor.id,
  })

  return (
    <Flex
      key={floor.id}
      size="tiny"
      padding="medium"
      className={cxFloorClassName}
    >
      <Text color="grey" style={{ color }}>
        {floor.code}
      </Text>
      <Text size="large">{floor.name}</Text>
    </Flex>
  )
}
