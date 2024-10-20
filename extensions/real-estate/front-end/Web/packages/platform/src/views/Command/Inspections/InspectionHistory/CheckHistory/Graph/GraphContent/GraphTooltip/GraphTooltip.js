import { Flex, Number, Text, Time, Tooltip } from '@willow/ui'
import numberUtils from '@willow/ui/utils/numberUtils'
import { useTranslation } from 'react-i18next'
import { useGraph } from '../../GraphContext'
import styles from './GraphTooltip.css'

export default function GraphTooltip({ target, lines, index }) {
  const graphContext = useGraph()
  const { t } = useTranslation()

  if (target == null || lines.length === 0) {
    return null
  }

  const graphs = [
    ...graphContext.contentRef.current.childNodes[0].querySelectorAll(
      '[data-graph]'
    ),
  ]
  const time = lines[0].item.x

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
          <Time value={time} />
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
                <Text>{line.equipment}</Text>
              </Flex>
            </Flex>
            {line.type === 'line' && (
              <Flex align="center">
                <Flex horizontal>
                  <Flex align="center middle" className={styles.value}>
                    <Text size="extraLarge" color="white">
                      <Number value={line.item.y} format="0.[00]" />
                    </Text>
                    <Text type="message" color="grey">
                      {line.yAxis ?? 'Value'}
                    </Text>
                  </Flex>
                  {line.item.min !== line.item.y && line.item.min != null && (
                    <Flex align="center middle" className={styles.value}>
                      <Text size="extraLarge" color="white">
                        <Number value={line.item.min} format="0.[00]" />
                      </Text>
                      <Text type="message" color="grey">
                        {t('labels.min')}
                      </Text>
                    </Flex>
                  )}
                  {line.item.max !== line.item.y && line.item.max != null && (
                    <Flex align="center middle" className={styles.value}>
                      <Text size="extraLarge" color="white">
                        <Number value={line.item.max} format="0.[00]" />
                      </Text>
                      <Text type="message" color="grey">
                        {t('labels.max')}
                      </Text>
                    </Flex>
                  )}
                </Flex>
              </Flex>
            )}
            {line.type === 'boolean' && (
              <Flex align="center" padding="medium">
                {line.item.true === 0 && (
                  <Text type="h4" color="white">
                    {t('plainText.off')}
                  </Text>
                )}
                {line.item.true > 0 && (
                  <Text type="h4" color="white">
                    {t('plainText.on')}
                  </Text>
                )}
                {line.item.true > 0 && line.item.false > 0 && (
                  <Text type="message" color="grey">
                    {t('interpolation.percentageOfTheTime', {
                      value:
                        numberUtils.format(
                          (line.item.true /
                            (line.item.true + line.item.false)) *
                            100,
                          '0'
                        ) || '',
                    })}
                  </Text>
                )}
              </Flex>
            )}
          </Flex>
        ))}
      </Flex>
    </Tooltip>
  )
}
