import { css } from 'twin.macro'
import { titleCase } from '@willow/common'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Flex, useDateTime, Text, Time } from '@willow/ui'
import { Button, Icon, useTheme } from '@willowinc/ui'
import styles from './Header.css'
import { useTimeSeriesGraph } from '../TimeSeriesGraphContext'

export default function Header({
  compact,
  isResetButtonVisible,
  onReset,
  graphZoom,
  enableLegend,
}) {
  const dateTime = useDateTime()
  const { t } = useTranslation()
  const timeSeriesGraph = useTimeSeriesGraph()
  const contentRef = useRef()

  const [labels, setLabels] = useState([])

  function refresh() {
    const width =
      timeSeriesGraph.headerRef.current?.scrollWidth -
      timeSeriesGraph.headerRef.current?.scrollLeft -
      138
    const minTime = new Date(timeSeriesGraph.times[0]).valueOf()
    const maxTime = new Date(timeSeriesGraph.times[1]).valueOf()

    let count = 2

    if (compact) {
      count = Math.max(2, 1 + 2 * Math.floor(width / 300)) * 2 - 1
    } else {
      if (width > 300) count = 3
      if (width > 700) count = 9
    }

    const nextLabels = Array.from(Array(count)).map((n, i) => {
      const percentage = i / (count - 1)
      return {
        left: width * percentage,
        time: dateTime(
          (maxTime - minTime) * percentage + minTime,
          timeSeriesGraph.timeZone
        ).format(),
      }
    })
    if (contentRef.current)
      contentRef.current.style.width = `${timeSeriesGraph.headerRef.current.clientWidth}px`
    setLabels(nextLabels)
  }

  useEffect(() => {
    refresh()
  }, [timeSeriesGraph.size, timeSeriesGraph.times])

  return (
    <div
      className={`${styles.header} ${graphZoom ? styles.zoom : ''}`}
      id="time-series-graph-header"
    >
      <div className={styles.content} ref={contentRef}>
        <div tw="relative" className={styles.spacer}>
          {isResetButtonVisible && (
            <Button
              prefix={<Icon icon="restart_alt" />}
              kind="secondary"
              onClick={onReset}
            >
              {t('plainText.reset')}
            </Button>
          )}
          {enableLegend && <DiagnosticOverlayLegend />}
        </div>

        <div
          className={styles.labelsWrapper}
          id="time-series-graph-labels-wrapper"
        >
          {labels.map((label, i) => (
            // eslint-disable-next-line react/no-array-index-key
            <Flex key={i}>
              <Text
                key={i} // eslint-disable-line
                size="tiny"
                className={styles.label}
                style={{
                  left: label.left,
                }}
              >
                {i % 2 === 0 && (
                  <>
                    <Flex padding="0 0 0 small" className={styles.time}>
                      <div>
                        <Time
                          timezone={timeSeriesGraph.timeZone}
                          value={label.time}
                          format="date"
                        />
                      </div>
                      <div>
                        <Time
                          timezone={timeSeriesGraph.timeZone}
                          value={label.time}
                          format="time"
                        />
                      </div>
                    </Flex>
                    <div className={styles.notch} />
                  </>
                )}
                {i % 2 !== 0 && (
                  <>
                    <div className={styles.notch} />
                  </>
                )}
              </Text>
            </Flex>
          ))}
        </div>
      </div>
    </div>
  )
}

const DiagnosticOverlayLegend = () => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const theme = useTheme()

  return (
    <div
      css={css`
        position: absolute;
        top: ${theme.spacing.s16};
        left: ${theme.spacing.s16};
        display: flex;
        white-space: nowrap;
        color: ${theme.color.neutral.fg.default};
        line-height: ${theme.spacing.s16};

        & > div:nth-child(odd) {
          height: ${theme.spacing.s16};
          width: ${theme.spacing.s16};
        }

        & > div:nth-child(even) {
          padding-left: ${theme.spacing.s8};
        }
      `}
    >
      <div
        css={css`
          background: rgba(176, 43, 51, 0.3);
        `}
      />
      <div>
        {titleCase({
          text: t('plainText.faulting'),
          language,
        })}
      </div>
      <div
        css={css`
          margin-left: ${theme.spacing.s16};
          background: repeating-linear-gradient(
            -45deg,
            transparent,
            transparent ${theme.spacing.s2},
            ${theme.color.core.gray.bg.muted.default} ${theme.spacing.s2},
            ${theme.color.core.gray.bg.muted.default} ${theme.spacing.s4}
          );
        `}
      />
      <div>
        {titleCase({
          text: t('plainText.insufficientData'),
          language,
        })}
      </div>
    </div>
  )
}
