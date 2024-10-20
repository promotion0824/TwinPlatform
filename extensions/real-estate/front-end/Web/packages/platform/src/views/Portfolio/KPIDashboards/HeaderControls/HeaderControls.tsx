/* eslint-disable camelcase */
import { Site } from '@willow/common/site/site/types'
import { DateRangePicker, Panel } from '@willow/ui'
import { QuickRangeOption } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { styled } from 'twin.macro'
import { AllSites } from '../../../../providers/sites/SiteContext'
import type { TimeSeriesFavorite } from '../../../TimeSeries/types'

const quickRangeOptions: QuickRangeOption[] = ['7D', '1M', '3M', '6M', '1Y']

export function DateRangeOptionsDropdown({
  handleDateRangeChange,
  dateRange,
  quickOptionSelected,
  onSelectQuickRange,
  onDayRangeChange,
  selectedDayRange,
  onBusinessHourRangeChange,
  selectedBusinessHourRange,
  onResetClick,
  hideBusinessHourRange,
  dataSegment,
}: {
  handleDateRangeChange?: (nextTimeRange) => void
  dateRange?: [string, string]
  quickOptionSelected?: string
  selectedDayRange?: string
  onSelectQuickRange?: (quickRangeOption: string) => void
  onDayRangeChange?: (dayRange: string) => void
  onBusinessHourRangeChange?: (selectedBusinessHourRange: string) => void
  selectedBusinessHourRange?: string
  onResetClick?: () => void
  hideBusinessHourRange?: boolean
  dataSegment?: string
}) {
  return (
    <DateRangePicker
      quickRangeOptions={quickRangeOptions}
      selectedQuickRange={quickOptionSelected}
      onSelectQuickRange={onSelectQuickRange}
      selectedDayRange={selectedDayRange}
      onDayRangeChange={onDayRangeChange}
      onBusinessHourRangeChange={onBusinessHourRangeChange}
      onResetClick={onResetClick}
      hideBusinessHourRange={hideBusinessHourRange}
      selectedBusinessHourRange={selectedBusinessHourRange}
      tw="w-[378px]"
      type="date-business-range"
      value={dateRange}
      onChange={handleDateRangeChange}
      data-segment={dataSegment}
    />
  )
}

export type User = {
  customer: { name: string; id: string }
  portfolios: { id: string; name: string }[]
  options: {
    timeMachineFavorites?: TimeSeriesFavorite[]
  }
  saveOptions?: (key: string, value: string) => void
  isCustomerAdmin?: boolean
  showAdminMenu?: boolean
  showPortfolioTab?: boolean
}

export type Analytics = {
  track: (
    eventName: string,
    property: {
      page?: string
      customer_name: string
      date_range_filter?: QuickRangeOption
      site?: Site | AllSites
      button_name?: string
    }
  ) => void
}

export type FeatureFlags = { hasFeatureToggle: (flag: string) => boolean }

export const dateRangeOptions = [
  { text: 'plainText.thisMonths', value: '0' },
  { text: 'plainText.thisYear', value: 'thisYear' },
  { text: 'plainText.previousMonth', value: '-1' },
  { text: 'plainText.previousThreeMonth', value: '-3' },
  { text: 'plainText.previousSixMonth', value: '-6' },
  { text: 'plainText.previousYear', value: 'prevYear' },
]

export const GridContainer = styled(Panel)({
  marginTop: 4,
  display: 'grid',
  gridTemplateRows: '0 230px 288px auto',
  gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
  gap: '8px',
  padding: '0 16px 12px 16px',
  height: '95%',
})

export const FlexRowContainer = styled(Panel)({
  marginTop: 4,
  padding: '8px 16px 4px 16px',
  height: '100%',
  overflowY: 'hidden',
})
