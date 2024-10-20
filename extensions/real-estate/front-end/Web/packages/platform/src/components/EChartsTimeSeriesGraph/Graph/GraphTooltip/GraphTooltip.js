/* eslint-disable complexity */
import { titleCase } from '@willow/common'
import {
  Flex,
  Number,
  Text,
  Time,
  Tooltip,
  useDateTime,
  useDuration,
} from '@willow/ui'
import numberUtils from '@willow/ui/utils/numberUtils'
import { Stack } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useTimeSeriesGraph } from '../../TimeSeriesGraphContext'
import styles from './GraphTooltip.css'

export default function GraphTooltip({ target, lines, index }) {
  const duration = useDuration()
  const dateTime = useDateTime()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const timeSeriesGraph = useTimeSeriesGraph()

  if (target == null || lines.length === 0) {
    return null
  }

  const graphs = [
    ...timeSeriesGraph.contentRef.current.querySelectorAll('[data-graph]'),
  ]
  const { time } = lines[0].item

  const toTime = time + duration(timeSeriesGraph.granularity).milliseconds()
  const endTime = dateTime(toTime, timeSeriesGraph.timeZone).format('time')

  return (
    <Tooltip
      target={target}
      yTarget={graphs[index]}
      position="left"
      showPointer={false}
      contentClassName={styles.tooltip}
    >
      <Flex
        horizontal
        align="middle"
        size="small"
        padding="medium"
        className={styles.header}
      >
        <Text type="h4" color="white">
          <Flex horizontal>
            <Time value={time} timezone={timeSeriesGraph.timeZone} />
            <span>-{endTime}</span>
          </Flex>
        </Text>
      </Flex>
      <Flex size="medium" padding="medium">
        {lines.map((line) => (
          <Flex key={line.pointId} className={styles.line}>
            <Flex horizontal>
              <Flex
                horizontal
                fill="header"
                align="middle"
                size="medium"
                className={styles.header}
              >
                <Flex horizontal align="middle" size="small">
                  <div className={styles.color} style={{ color: line.color }} />
                  <Text>{line.name}</Text>
                </Flex>
              </Flex>
              <Flex
                horizontal
                fill="header"
                align="middle"
                size="medium"
                padding="medium"
                className={styles.equipment}
              >
                <Text>{line.assetName}</Text>
              </Flex>
            </Flex>
            {line.type === 'analog' && (
              <Flex align="center">
                <Flex horizontal>
                  <Flex align="center middle" className={styles.value}>
                    <Text size="extraLarge" color="white">
                      <Number value={line.item.average} format="0.[00]" />
                    </Text>
                    <Text type="message" color="grey">
                      {line.unit ?? 'Value'}
                    </Text>
                  </Flex>
                  {line.item.minimum !== line.item.average &&
                    line.item.minimum != null && (
                      <Flex align="center middle" className={styles.value}>
                        <Text size="extraLarge" color="white">
                          <Number value={line.item.minimum} format="0.[00]" />
                        </Text>
                        <Text type="message" color="grey">
                          {t('labels.min')}
                        </Text>
                      </Flex>
                    )}
                  {line.item.maximum !== line.item.average &&
                    line.item.maximum != null && (
                      <Flex align="center middle" className={styles.value}>
                        <Text size="extraLarge" color="white">
                          <Number value={line.item.maximum} format="0.[00]" />
                        </Text>
                        <Text type="message" color="grey">
                          {t('labels.max')}
                        </Text>
                      </Flex>
                    )}
                </Flex>
              </Flex>
            )}
            {line.type === 'binary' && (
              <Flex align="center" padding="medium">
                {line.item.onCount === 0 && (
                  <Text type="h4" color="white">
                    {line.item?.isDiagnosticPoint
                      ? _.capitalize(t('plainText.pass'))
                      : t('plainText.off')}
                  </Text>
                )}
                {line.item.onCount > 0 && (
                  <Text type="h4" color="white">
                    {line.item?.isDiagnosticPoint
                      ? _.capitalize(t('plainText.fail'))
                      : t('plainText.on')}
                  </Text>
                )}
                {line.item.onCount > 0 && line.item.offCount > 0 && (
                  <Text type="message" color="grey">
                    {t('interpolation.percentageOfTheTime', {
                      value:
                        numberUtils.format(
                          (line.item.onCount /
                            (line.item.onCount + line.item.offCount)) *
                            100,
                          '0'
                        ) || '',
                    })}
                  </Text>
                )}
              </Flex>
            )}
            {line.type === 'multiState' && (
              <Stack align="center" p="s8">
                {Object.entries(line.item.state).map(([key, value]) => {
                  const total = Object.values(line.item.state).reduce(
                    (a, b) => a + b
                  )
                  const percentage = value / total
                  return (
                    <Stack align="center" p="s2" key={key}>
                      <Text type="h4" color="white">
                        {titleCase({ text: line.valueMap[key], language })}
                      </Text>
                      {value >= 0 && percentage !== 1 && (
                        <Text type="message" color="grey">
                          {t('interpolation.percentageOfTheTime', {
                            value:
                              numberUtils.format(percentage * 100, '0') || '',
                          })}
                        </Text>
                      )}
                    </Stack>
                  )
                })}
              </Stack>
            )}
          </Flex>
        ))}
      </Flex>
    </Tooltip>
  )
}
