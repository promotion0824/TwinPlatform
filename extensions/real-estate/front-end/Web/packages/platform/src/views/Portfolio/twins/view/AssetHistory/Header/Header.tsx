import { styled } from 'twin.macro'
import { Select, Option, DatePicker, DatePickerButton } from '@willow/ui'
import { FilterType } from '../provider/AssetHistoryProvider'

/**
 * A component that is used to get table data for different date ranges
 * for insights, scheduled tickets, standard tickets etc
 */

function Header({
  typeOptions,
  filterType,
  filterDateRange,
  onDateRangeChange,
  dateRangePickerOptions,
  onFilterTypeChange,
}: {
  filterType: FilterType
  dateRangePickerOptions: Record<
    string,
    { label: string; handleDateRangePick: () => void }
  >
  typeOptions: Record<FilterType, string>
  onFilterTypeChange: (val: FilterType) => void
  filterDateRange: [string, string]
  onDateRangeChange: (nextDateRange: [string, string]) => void
}) {
  return (
    <>
      <InputContainer>
        <StyledSelect
          value={typeOptions[filterType]}
          onChange={(val: FilterType) => onFilterTypeChange(val)}
        >
          {Object.entries(typeOptions).map(([key, label]) => (
            <Option key={key} value={key}>
              {label}
            </Option>
          ))}
        </StyledSelect>

        <StyledDatePicker
          type="date-time-range"
          value={filterDateRange}
          onChange={onDateRangeChange}
          helper={
            <>
              {Object.entries(dateRangePickerOptions).map(
                ([key, { label, handleDateRangePick }]) => (
                  <DatePickerButton key={key} onClick={handleDateRangePick}>
                    {label}
                  </DatePickerButton>
                )
              )}
            </>
          }
        />
      </InputContainer>
    </>
  )
}

export default Header

const StyledSelect = styled(Select)({ width: '160px' })

const InputContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'row',
  flexWrap: 'wrap',
  gap: theme.spacing.s12,
}))

const StyledDatePicker = styled(DatePicker)({
  width: '278px',
  minWidth: 'unset',
})
