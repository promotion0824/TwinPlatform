import { styled } from 'twin.macro'

import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import {
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
} from '@willow/ui'

import { DateRangeOptionsDropdown } from '../../Portfolio/KPIDashboards/HeaderControls/HeaderControls'

interface PerformanceViewHeaderProps {
  disableDatePicker?: boolean
  quickOptionSelected?: string
  onQuickOptionChange?: (quickOptionSelected: string) => void
  handleDateRangeChange: (params: ParamsDict) => void
  selectedDayRange?: DatePickerDayRangeOptions
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
  onDayRangeChange?: (selectedDayRange: DatePickerDayRangeOptions) => void
  onBusinessHourRangeChange?: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  onResetClick?: () => void
  hideBusinessHourRange?: boolean
  dateRange: [string, string]
}
export default function PerformanceViewHeader({
  disableDatePicker = false,
  quickOptionSelected,
  onQuickOptionChange,
  handleDateRangeChange,
  selectedDayRange,
  selectedBusinessHourRange,
  onDayRangeChange,
  onBusinessHourRangeChange,
  onResetClick,
  hideBusinessHourRange,
  dateRange,
}: PerformanceViewHeaderProps) {
  return (
    <HeaderContainer $isAlignRight={disableDatePicker}>
      {!disableDatePicker && (
        <DateRangeOptionsDropdown
          quickOptionSelected={quickOptionSelected}
          onSelectQuickRange={onQuickOptionChange}
          selectedDayRange={selectedDayRange}
          onDayRangeChange={onDayRangeChange}
          onBusinessHourRangeChange={onBusinessHourRangeChange}
          onResetClick={onResetClick}
          selectedBusinessHourRange={selectedBusinessHourRange}
          dateRange={dateRange}
          handleDateRangeChange={handleDateRangeChange}
          hideBusinessHourRange={hideBusinessHourRange}
          dataSegment="Dashboard Calendar Expanded"
        />
      )}
    </HeaderContainer>
  )
}

const HeaderContainer = styled.div<{ $isAlignRight: boolean }>(
  ({ theme, $isAlignRight }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: `${$isAlignRight ? 'flex-end' : 'space-between'}`,
    padding: theme.spacing.s16,
    borderBottom: `1px solid ${theme.color.neutral.border.default}`,
    backgroundColor: `${theme.color.neutral.bg.panel.default}`,
  })
)
