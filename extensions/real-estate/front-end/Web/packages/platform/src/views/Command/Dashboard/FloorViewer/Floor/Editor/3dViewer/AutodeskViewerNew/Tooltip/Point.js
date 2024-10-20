import { Flex, Number, Icon, Text, Time } from '@willow/ui'
import styles from './Point.css'

export default function Point({ point }) {
  return (
    <Flex key={point.id} size="small" padding="medium" className={styles.point}>
      <Flex horizontal fill="header" align="middle" size="medium">
        <Text type="message" size="tiny">
          {point.tag}
        </Text>
        {point.liveDataValue != null && (
          <Text type="message" size="tiny">
            <Time value={point.liveDataTimestamp} format="ago" />
          </Text>
        )}
      </Flex>
      {point.liveDataValue != null && (
        <>
          {point.unit === 'Bool' && (
            <Text type="message" color="white">
              {point.liveDataValue === 0 ? 'Off' : 'On'}
            </Text>
          )}
          {point.unit !== 'Bool' && (
            <Text color="white">
              <Flex horizontal size="small">
                <Number value={point.liveDataValue} format="0.[00]" />
                <span>{point.unit}</span>
              </Flex>
            </Text>
          )}
        </>
      )}
      {point.liveDataValue == null && (
        <Flex align="center" size="small">
          <Icon icon="notFound" />
          <Text type="message" size="tiny">
            No data found in the last hour
          </Text>
        </Flex>
      )}
    </Flex>
  )
}
