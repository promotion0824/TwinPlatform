import { DatePicker } from '@willow/ui'
import { useMemo } from 'react'
import { styled } from 'twin.macro'
import { useTimeSeries } from '../../components/TimeSeries/TimeSeriesContext'
import TimeZoneSelect from '../../components/TimeZoneSelect/TimeZoneSelect'
import styles from './TimeSeriesHeader.css'

const quickRangeOptions = ['24H', '48H', '7D', 'thisMonth', 'prevMonth', '3M']

const DatePickerContainer = styled.div({
  maxWidth: '100%',
})

export default function EChartsTimeSeriesHeader({
  quickRange,
  onQuickRangeChange,
  times,
  onTimesChange,
  timeZoneOption,
  timeZone,
  onTimeZoneChange,
}) {
  const timeSeries = useTimeSeries()

  const siteIds = useMemo(
    () => timeSeries.assets.map((asset) => asset.siteId),
    [timeSeries.assets]
  )

  return (
    <DatePickerContainer>
      <DatePicker
        type="date-time-range"
        className={
          quickRange
            ? styles.headerButton
            : `${styles.headerButton} ${styles.active}`
        }
        quickRangeOptions={quickRangeOptions}
        selectedQuickRange={quickRange}
        onSelectQuickRange={onQuickRangeChange}
        value={times}
        onChange={(pickedTimes, isCustomRange) => {
          onTimesChange(
            pickedTimes,
            isCustomRange /* Send analytics, only when date range custom */
          )
        }}
        // The backend does not allow us to retrieve data over a range of
        // more than 371 days (which equals 53 weeks).
        maxDays={371}
        timezone={timeZone}
        timezoneSelector={
          <TimeZoneSelect
            value={timeZoneOption}
            onChange={onTimeZoneChange}
            siteIds={siteIds}
          />
        }
        data-segment="Time Series Calendar Expanded"
      />
    </DatePickerContainer>
  )
}
