import { InsightTab } from '@willow/common/insights/insights/types'
import {
  Flex,
  useAnalytics,
  useDateTime,
  useDuration,
  useUser,
} from '@willow/ui'
import { getDateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions.tsx'
import cx from 'classnames'
import { useEffect, useRef, useState } from 'react'
import { useHistory } from 'react-router-dom'
import { useSites } from '../../providers'
import { useSelectedPoints } from '../MiniTimeSeries/SelectedPointsContext'
import { replaceTimeZoneForDateTimeRange } from '../TimeSeries/utils/dateTimeUtils'
import TimeSeriesGraph, { getDefaultGranularity } from '../TimeSeriesGraph'
import { getTimeZone, useTimeZoneInfo } from '../TimeZoneSelect/useGetTimeZones'
import Header from './Header/Header'
import styles from './MiniTimeSeriesComponent.css'
import { MiniTimeSeriesContext } from './MiniTimeSeriesContext'

const defaultType = 'grouped'
const defaultQuickRange = '48H'

export default function EChartsMiniTimeSeriesComponent({
  times: defaultTimes,
  siteEquipmentId,
  className,
  equipmentName,
  hideEquipments,
  shadedDurations,
  twinInfo,
  isDefaultQuickRange = true,
  isViewingDiagnostic = false,
  insightTab,
  period,
  onPeriodChange,
  diagnosticBoundaries,
}) {
  const sites = useSites()
  const history = useHistory()
  const dateTime = useDateTime()
  const [now, setNow] = useState()
  const duration = useDuration()
  const analytics = useAnalytics()
  const [quickRange, setQuickRange] = useState(
    isDefaultQuickRange ? defaultQuickRange : undefined
  )

  const [siteId, assetId] = siteEquipmentId.split('_')
  const siteTimeZoneId = sites?.find((site) => site.id === siteId)?.timeZoneId
  const [timeZoneOption, setTimeZoneOption] = useState()
  const timeZoneInfo = useTimeZoneInfo(
    siteTimeZoneId ?? timeZoneOption?.timeZoneId
  )

  const contentRef = useRef()
  const user = useUser()
  const {
    pointIds: selectedPointIds,
    onUpdateParams,
    loadingPointIds,
    points,
  } = useSelectedPoints()

  const [times, setTimes] = useState(() => {
    const to = dateTime.now().format()
    const from = dateTime(to).addHours(-48).format()
    return defaultTimes ?? [from, to]
  })
  const [granularity, setGranularity] = useState(() =>
    duration(getDefaultGranularity(times)).toISOString()
  )
  const [type, setType] = useState(defaultType)

  const timeZone = timeZoneInfo && getTimeZone(timeZoneInfo)

  useEffect(() => {
    const [start, end] = times
    onUpdateParams({
      start,
      end,
      interval: duration(granularity).toDotnetString(),
    })
  }, [times, granularity])

  function handleGoToTimeSeries() {
    user.saveOptions('timeSeriesState', {
      times,
      type,
      granularity,
      siteEquipmentIds: [siteEquipmentId],
      siteId,
      sitePointIds: selectedPointIds,
      quickSelectTimeRange: quickRange,
      timeZoneOption,
      timeZone,
    })
    history.push('/time-series')
  }

  function handleTimesChange(updatedTimes, isCustomRange = false) {
    let nextTimes = updatedTimes

    if (!updatedTimes?.length) {
      // Handles reset - Reset timeZone, quick range and time range (based on timeZone & quick range).
      setTimeZoneOption(null)
      setQuickRange(defaultQuickRange)
      nextTimes = getDateTimeRange(dateTime.now(), defaultQuickRange)
    } else if (isCustomRange) {
      let range
      if (nextTimes?.length === 2) {
        const date1 = new Date(nextTimes[0])
        const date2 = new Date(nextTimes[1])
        const diffTime = Math.abs(date2 - date1)
        range = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      }
      analytics.track('Time Series Calendar Date Range', {
        range_of_days: range,
      })
    }

    setTimes(nextTimes)
    onPeriodChange?.(nextTimes.join(' - '))
    setGranularity(duration(getDefaultGranularity(nextTimes)).toISOString())
  }

  function handleTimeZoneChange(nextTimeZoneOption, nextTimeZoneInfo) {
    setTimeZoneOption(nextTimeZoneOption)
    analytics.track('DateTimeRange Timezone changed')

    const nextTimeZone = nextTimeZoneInfo && getTimeZone(nextTimeZoneInfo)

    setTimes(replaceTimeZoneForDateTimeRange(times, timeZone, nextTimeZone))
  }

  const timeSeriesPeriod =
    insightTab === InsightTab.Diagnostics &&
    period &&
    isViewingDiagnostic &&
    twinInfo?.diagnosticStart &&
    twinInfo?.diagnosticEnd
      ? [twinInfo.diagnosticStart, twinInfo.diagnosticEnd]
      : times

  const context = {
    contentRef,
    now,
    times: timeSeriesPeriod,
    granularity,
    siteEquipmentId,
  }

  useEffect(() => {
    setNow(dateTime.now().format())
  }, [times])

  return (
    <MiniTimeSeriesContext.Provider value={context}>
      Version 2
      <Flex fill="content" className={cx(styles.timeSeries, className)}>
        <Header
          quickRange={quickRange}
          onQuickRangeChange={setQuickRange}
          times={timeSeriesPeriod}
          onTimesChange={handleTimesChange}
          siteId={siteId}
          assetId={assetId}
          equipmentDisabled={hideEquipments}
          onTimeSeriesClick={handleGoToTimeSeries}
          equipmentName={equipmentName}
          timeZoneOption={timeZoneOption}
          timeZone={timeZone}
          onTimeZoneChange={handleTimeZoneChange}
          twinInfo={twinInfo}
        />
        <div className={styles.content}>
          <TimeSeriesGraph
            compact
            onTimesChange={(nextTimes) => {
              handleTimesChange(nextTimes, true)
              setQuickRange(null)
            }}
            granularity={granularity}
            type={type}
            onTypeChange={setType}
            times={timeSeriesPeriod}
            onGranularityChange={setGranularity}
            pointsData={points}
            loadingSitePointIds={loadingPointIds}
            timeZone={timeZone}
            shadedDurations={shadedDurations}
            diagnosticBoundaries={diagnosticBoundaries}
          />
        </div>
      </Flex>
    </MiniTimeSeriesContext.Provider>
  )
}
