import { DateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { useCurrentTimeRange, useInspections } from '../InspectionsProvider'
import InspectionCheckHistoryDatePicker from './InspectionCheckHistoryDatePicker/InspectionCheckHistoryDatePicker.js'

export default function TimeRangePicker() {
  const { sharedTimeRange, setSharedTimeRange } = useInspections()
  const currentTimeRange = useCurrentTimeRange()

  const handleTimeRangeChange = (nextTimes: DateTimeRange | []) => {
    setSharedTimeRange(nextTimes.length === 0 ? currentTimeRange : nextTimes)
  }

  return (
    <InspectionCheckHistoryDatePicker
      css={{ height: 28 /* same height as other design-system Button */ }}
      times={sharedTimeRange}
      onTimesChange={handleTimeRangeChange}
    />
  )
}
