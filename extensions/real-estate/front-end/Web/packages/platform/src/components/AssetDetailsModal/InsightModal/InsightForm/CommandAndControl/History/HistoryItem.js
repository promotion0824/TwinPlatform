import { Flex, Header, Number, Panel, Pill, Text, Time } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './HistoryItem.css'

export default function HistoryItem({ command }) {
  const { t } = useTranslation()
  const ONE_DAY = 60 * 24
  const ONE_HOUR = 60

  const days = Math.floor(command.desiredDurationMinutes / ONE_DAY)
  const hours = Math.floor(
    (command.desiredDurationMinutes % ONE_DAY) / ONE_HOUR
  )
  const minutes = Math.floor(command.desiredDurationMinutes % ONE_HOUR)

  const time = `${days}d ${hours}h ${minutes}m`

  let color = 'red'
  if (
    command.status === 'created' ||
    command.status === 'submitted' ||
    command.status === 'active' ||
    command.status === 'completed'
  ) {
    color = 'green'
  }

  return (
    <Panel>
      <Header fill="header">
        <Text>
          {command.createdBy.firstName} {command.createdBy.lastName}
        </Text>
        <Time value={command.createdAt} />
      </Header>
      <Flex horizontal fill="header">
        <Flex size="medium" padding="large">
          <Flex horizontal>
            <Text width="small">{t('labels.currentSetpoint')}:</Text>
            <Text color="white">{command.originalValue}</Text>
          </Flex>
          <Flex horizontal>
            <Text width="small">{t('labels.newSetpoint')}:</Text>
            <Text color="white">{command.desiredValue}</Text>
          </Flex>
          <Flex horizontal>
            <Text width="small" className={styles.type}>
              {command.type}:
            </Text>
            <Text color="white">
              <Number value={command.currentReading} format=",.00" />
              <span> {command.unit}</span>
            </Text>
          </Flex>
          <Flex horizontal>
            <Text width="small">{t('labels.duration')}:</Text>
            <Text color="white">{time}</Text>
          </Flex>
        </Flex>
        <Flex padding="large">
          <Pill color={color}>{command.status}</Pill>
        </Flex>
      </Flex>
    </Panel>
  )
}
