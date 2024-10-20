import { useCallback, useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import Text from 'components/Text/Text'
import AnimatedNumber from './AnimatedNumber'
import styles from './GaugeChart.css'

/**
 * Display a gauge chart.
 *
 * The gauge is always displayed, regardless of whether data exists.
 *
 * If a value is available (via the `metric` prop), it is displayed. Otherwise
 * the message "No data available" is displayed, but only if `isLoading` is
 * false.
 */
export default function GaugeChart({
  chartRef,
  metric: { value, color, size },
  icon,
  textClassName,
  showLabels,
  isLoading,
}) {
  const { t } = useTranslation()
  const [chartWidth, setChartWidth] = useState(0)
  const resizeGraph = useCallback(() => {
    if (chartRef && chartRef.current) {
      const { height, width } = chartRef.current.getBoundingClientRect()
      // We need to set chart width shorter for 72px if labels are shown
      let calcWidth = width - (showLabels ? 72 : 0)
      if (calcWidth / 2 > height) {
        calcWidth = height * 2
      }
      setChartWidth(calcWidth)
    }
  }, [chartRef])
  const hasValue = value != null

  useEffect(() => {
    resizeGraph()
    window.addEventListener('resize', resizeGraph)
    return () => {
      window.removeEventListener('resize', resizeGraph)
    }
  }, [resizeGraph])

  const rotate = value * 180 - 180

  const cxClassName = cx(styles.arcValue, {
    [styles.colorGreen]: hasValue && color === 'green',
    [styles.colorYellow]: hasValue && color === 'yellow',
    [styles.colorOrange]: hasValue && color === 'orange',
    [styles.colorRed]: hasValue && color === 'red',
    [styles.sizeLarge]: size === 'large',
  })

  return (
    <Flex horizontal className={styles.ctn}>
      {showLabels && <span className={styles.leftLabel}>0%</span>}
      <div
        className={cxClassName}
        style={{ width: chartWidth, height: chartWidth / 2 }}
      >
        <div className={styles.track} />
        <div
          className={styles.trackValue}
          style={{ transform: `rotate(${rotate}deg)` }}
        >
          {hasValue && (
            <>
              <div className={styles.trackValueBackground} />
              <div className={styles.trackValueBorder} />
            </>
          )}
        </div>
        <div className={styles.mask} />
        <Text
          size="massive"
          align="center"
          color={color}
          className={cx(styles.text, textClassName)}
        >
          {icon && hasValue && icon}
          {hasValue && <AnimatedNumber value={value} />}
        </Text>
        {!isLoading && !hasValue && (
          <div className={styles.noDataText}>
            {t('plainText.noDataAvailable')}
          </div>
        )}
      </div>
      {showLabels && <span className={styles.rightLabel}>100%</span>}
    </Flex>
  )
}
