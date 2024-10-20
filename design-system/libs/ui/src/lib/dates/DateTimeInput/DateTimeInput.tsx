import dayjs from 'dayjs'
import { identity, isArray } from 'lodash'
import {
  Dispatch,
  ReactNode,
  SetStateAction,
  forwardRef,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'

import { Button } from '../../buttons/Button'
import { TextInput } from '../../inputs/TextInput'
import { BaseProps as TextInputBaseProps } from '../../inputs/TextInput/TextInput'
import { Popover } from '../../overlays/Popover'
import { WillowStyleProps } from '../../utils/willowStyleProps'
import { DatePicker, DatePickerProps } from '../DatePicker'
import { TimezoneSelector, TimezoneSelectorProps } from '../TimezoneSelector'
import { getLocalTimezone } from '../TimezoneSelector/utils'
import {
  QuickActionSelector,
  QuickActionSelectorProps,
  SingleDateInput,
  SingleTimeInput,
} from './components'
import {
  Container,
  DateInputContainer,
  DisplayedValueContainer,
  FooterContainer,
  HorizontalContainer,
  QuickActionContainer,
  RowContainer,
} from './styles'
import { DateTimeType, DateValue } from './types'
import {
  calculateInitialValues,
  checkIfDateTimeValid,
  checkIfTimezoneValid,
  getDefaultPlaceholderByType,
  getDisplayText,
  getOutputDates,
  getTimeFromDate,
  isValidTimezone,
} from './utils'

/**
 * This is a temporary solution to fix the type errors when using DateTimeInputValue<T>,
 * eventually we should remove this and use DateTimeInputValue<T> directly.
 */
type TempDateType = Date | [Date, Date]

export interface DateTimeInputProps<T extends DateTimeType = 'date'>
  extends WillowStyleProps,
    Omit<
      TextInputBaseProps,
      keyof WillowStyleProps | 'value' | 'defaultValue' | 'onChange'
    > {
  type?: T | DateTimeType

  /** Value of the DateTimeInput component. */
  value?: TempDateType
  onChange?: (value: TempDateType) => void

  /**
   * The default timezone that will be applied.
   * If not specified, it will be default as system timezone.
   * The selected Date value will be date time of this timezone.
   */
  defaultTimezone?: string | null
  timezone?: string | null
  onTimezoneChange?: (timezone: string | null) => void

  // props for TextInput trigger
  readOnly?: TextInputBaseProps['readOnly']
  disabled?: TextInputBaseProps['disabled']
  label?: TextInputBaseProps['label']
  description?: TextInputBaseProps['description']
  error?: TextInputBaseProps['error']
  placeholder?: TextInputBaseProps['placeholder']
  prefix?: TextInputBaseProps['prefix']
  suffix?: TextInputBaseProps['suffix']

  // customizable labels
  /** Label for single date input */
  dateLabel?: string
  /** Label for single time input */
  timeLabel?: string
  /** Label for start date input */
  startDateLabel?: string
  /** Label for end date input */
  endDateLabel?: string
  /** Label for start time input */
  startTimeLabel?: string
  /** Label for end time input */
  endTimeLabel?: string
  /** Label for cancel button */
  cancelButtonLabel?: string
  /** Label for apply button */
  applyButtonLabel?: string

  // props for QuickActionSelector
  quickActionProps?: Partial<QuickActionSelectorProps<T>>

  minDate?: DatePickerProps['minDate']
  maxDate?: DatePickerProps['maxDate']
  defaultCalendarDate?: DatePickerProps['date']
  // props for singer DatePicker
  datePickerProps?: Partial<
    Exclude<DatePickerProps<'default'>, 'onChange' | 'type'>
  >
  // props for DateRangePicker
  dateRangePickerProps?: Partial<
    Exclude<DatePickerProps<'range'>, 'onChange' | 'type'>
  >

  /**
   * The customizable section for any addon components for example hours filter.
   * It won't impact the value of DateTimeInput, because any selected value
   * in this section is already managed by user.
   */
  customizeSection?: ReactNode
  /**
   * Will accept default date time string of the DateTimeInput component,
   * and update the final result string displaying in summary section
   * and in trigger component.
   */
  customizeValueText?: (value: string) => string
  /**
   * An additional validation function that will validate the selected
   * values before enabling the Apply button.
   */
  onValidate?: (value: DateValue | [DateValue, DateValue]) => boolean
  /**
   * Customizable function that will be called when the
   * cancel button is clicked. Used for resetting any custom state.
   */
  onCancel?: () => void
  /**
   * Customizable function that will be called when the
   * apply button is clicked. Used for update the value.
   */
  onApply?: () => void

  /** props for TimezoneSelector */
  timezoneSelectorProps?: Partial<TimezoneSelectorProps>
}

/**
 * `DateTimeInput` is a component that allows user to input date and time or date time range.
 */
export const DateTimeInput = forwardRef<HTMLDivElement, DateTimeInputProps>(
  (
    {
      type = 'date',
      value,
      onChange,
      defaultTimezone,
      timezone,
      onTimezoneChange,
      minDate,
      maxDate,
      defaultCalendarDate,

      // QuickActionSelector
      quickActionProps,
      // single DatePicker
      datePickerProps,
      // DateRangePicker
      dateRangePickerProps,

      // labels
      dateLabel = 'Date',
      timeLabel = 'Time',
      startDateLabel = 'Start Date',
      endDateLabel = 'End Date',
      startTimeLabel = 'Start Time',
      endTimeLabel = 'End Time',
      cancelButtonLabel = 'Cancel',
      applyButtonLabel = 'Apply',

      // customizations
      onValidate,
      onCancel,
      onApply,
      customizeValueText = identity,
      customizeSection,

      // TextInput trigger, rest props will pass to it
      placeholder = getDefaultPlaceholderByType(type),

      timezoneSelectorProps,
      ...restProps
    },
    ref
  ) => {
    // only check if a non-null value is provided
    if (defaultTimezone != null && !isValidTimezone(defaultTimezone)) {
      console.error(`Invalid default timezone provided: ${timezone}.`)
    }
    if (timezone != null && !isValidTimezone(timezone)) {
      console.error(`Invalid timezone provided: ${timezone}.`)
    }

    const [popoverOpened, setPopoverOpened] = useState(false)

    const onDateInputChange = (date: DateValue) => {
      setCalendarDisplayDate(date ?? undefined)
    }

    const localTimezone = getLocalTimezone()
    // priority: timezone > defaultTimezone > localTimezone
    const initialTimezone = checkIfTimezoneValid(timezone)
      ? timezone
      : checkIfTimezoneValid(defaultTimezone)
      ? defaultTimezone
      : localTimezone
    const {
      dates: initialDates,
      time: initialTime,
      inputValue: initialInputValue,
    } = useMemo(
      () => calculateInitialValues(type, initialTimezone, value),
      [initialTimezone, type, value]
    )
    const initialCalendarDisplayDate: Date | undefined =
      defaultCalendarDate ?? initialDates[1] ?? initialDates[0]

    const [inputValue, setInputValue] = useState<string | undefined>(
      initialInputValue
    )
    const [selectedDates, setSelectedDates] = useState<
      [DateValue] | [DateValue, DateValue] | []
    >(initialDates)
    // cannot include selectedTime in selectedDates, because if user select the
    // time first, we do not have a date to apply the time yet.
    const [selectedTime, setSelectedTime] = useState<
      [string | undefined] | [string | undefined, string | undefined] | []
    >(initialTime)
    const [selectedTimezone, setSelectedTimezone] = useState<string | null>(
      initialTimezone
    )
    // For the default display month and year for calendar.
    // Because typing the date in DateInput outside current month will
    // not switch to the calendar display.
    const [calendarDisplayDate, setCalendarDisplayDate] = useState<
      Date | undefined
    >(initialCalendarDisplayDate)
    const resetStates = useCallback(() => {
      setInputValue(initialInputValue)
      setSelectedDates(initialDates)
      setSelectedTime(initialTime)
      setSelectedTimezone(initialTimezone)
      setCalendarDisplayDate(initialCalendarDisplayDate)
    }, [
      initialCalendarDisplayDate,
      initialDates,
      initialInputValue,
      initialTime,
      initialTimezone,
    ])

    const selectedStartDateRef = useRef<HTMLButtonElement | null>(null)
    const rangeCalendarRef = useRef<HTMLDivElement | null>(null)
    useEffect(
      () => {
        // find the selected start date cell element and store it's ref
        if (selectedDates[0] && rangeCalendarRef?.current) {
          selectedStartDateRef.current =
            rangeCalendarRef.current.querySelector(
              `[aria-label="${dayjs(selectedDates[0]).format('D MMMM YYYY')}"]`
            ) ?? null
        }
      },
      // a more specific reference list, value of selectedDates[0] is important here
      // eslint-disable-next-line react-hooks/exhaustive-deps
      [selectedDates[0], selectedStartDateRef.current]
    )

    useEffect(
      () => {
        // when there is one date selected, and user clicked something outside the calendar,
        // we will mock the click of the start date in DateRangePicker to deselect the start date
        function handleClickOutsideCalendar(event: MouseEvent) {
          // if clicked outside the calendar
          if (
            rangeCalendarRef.current &&
            !rangeCalendarRef.current.contains(event.target as Node)
          ) {
            if (
              // To check whether if start date is selected,
              // cannot use selectedDates[0] which will be buggy here
              selectedStartDateRef?.current?.getAttribute('data-selected') ===
                'true' &&
              // only deselect start when not end is selected
              !selectedDates[1]
            ) {
              // deselect start date
              selectedStartDateRef?.current.click()
            }
          }
        }

        document.addEventListener('mousedown', handleClickOutsideCalendar)
        return () => {
          document.removeEventListener('mousedown', handleClickOutsideCalendar)
        }
      },
      // a more specific reference list
      // eslint-disable-next-line react-hooks/exhaustive-deps
      [
        rangeCalendarRef.current,
        selectedStartDateRef.current,
        // only check boolean value is enough
        // eslint-disable-next-line react-hooks/exhaustive-deps
        !!selectedDates[1],
      ]
    )

    const handleQuickActionClick = useCallback(
      (newDate: Date | [Date, Date]) => {
        if (isArray(newDate)) {
          setSelectedDates(newDate)
          setCalendarDisplayDate(newDate[1])
          if (type.includes('time')) {
            setSelectedTime([
              getTimeFromDate(newDate[0]),
              getTimeFromDate(newDate[1]),
            ])
          }
        } else {
          setSelectedDates([newDate])
          setCalendarDisplayDate(newDate)
          if (type.includes('time')) {
            setSelectedTime([getTimeFromDate(newDate)])
          }
        }
      },
      [type]
    )
    const handleSingleDateChange = useCallback((date: DateValue) => {
      onDateInputChange(date)
      setSelectedDates([date])
    }, [])
    const handleStartDateChange = useCallback(
      (date: DateValue) => {
        onDateInputChange(date)
        setSelectedDates((currentDates) => [date, currentDates[1] ?? null])
        if (type === 'date-time-range') {
          setDefaultStartOrEndTime(0, setSelectedTime)
        }
      },
      [type]
    )
    const handleEndDateChange = useCallback(
      (date: DateValue) => {
        onDateInputChange(date)
        setSelectedDates((currentDates) => [currentDates[0] ?? null, date])

        if (type === 'date-time-range') {
          setDefaultStartOrEndTime(1, setSelectedTime)
        }
      },
      [type]
    )
    const handleSingleDatePickerChange = useCallback((date: DateValue) => {
      setSelectedDates([date])
    }, [])
    const handleDateRangePickerChange = useCallback(
      (dates: [DateValue, DateValue]) => {
        setSelectedDates(dates)
        if (type === 'date-time-range') {
          if (dates[0]) {
            setDefaultStartOrEndTime(0, setSelectedTime)
          }
          if (dates[1]) {
            setDefaultStartOrEndTime(1, setSelectedTime)
          }
        }
      },
      [type]
    )

    const handleTimezoneChange = useCallback(
      (newTimezone: string | null) => {
        setSelectedTimezone(newTimezone)
        onTimezoneChange?.(newTimezone)
      },
      [onTimezoneChange]
    )

    const isValid =
      checkIfTimezoneValid(selectedTimezone) &&
      !!checkIfDateTimeValid(type, selectedDates, selectedTime) &&
      (onValidate?.(
        getOutputDates({
          type,
          date: selectedDates as [Date, Date] | [Date],
          time: selectedTime as [string, string] | [string],
          timezone: selectedTimezone as string,
        })
      ) ??
        true)

    const getValueText = () =>
      customizeValueText(
        getDefaultValueText(isValid, type, selectedDates, selectedTime)
      )

    /** Will only be called when it is valid */
    function handleApplyClick() {
      setInputValue(getValueText())
      onChange?.(
        getOutputDates({
          type,
          date: selectedDates as [Date, Date] | [Date],
          time: selectedTime as [string, string] | [string],
          timezone: selectedTimezone as string,
        })
      )
      setPopoverOpened(false)
      onApply?.()
    }

    function handleCancelClick() {
      setPopoverOpened(false)
      resetStates()
      onCancel?.()
    }

    const isSingleType = type === 'date' || type === 'date-time'
    const isRangeType = type === 'date-range' || type === 'date-time-range'

    return (
      <Popover
        position="bottom-start"
        opened={popoverOpened}
        onChange={setPopoverOpened}
      >
        <Popover.Target>
          <TextInput
            placeholder={placeholder}
            value={inputValue}
            onClick={() => setPopoverOpened(true)}
            {...restProps}
          />
        </Popover.Target>
        <Popover.Dropdown>
          <Container ref={ref}>
            <HorizontalContainer>
              <DateInputContainer data-testid="date-inputs-container">
                {isSingleType && (
                  <>
                    <RowContainer>
                      <SingleDateInput
                        label={dateLabel}
                        selectedDate={selectedDates[0]}
                        onDateChange={handleSingleDateChange}
                      />
                    </RowContainer>
                    {type === 'date-time' && (
                      <RowContainer>
                        <SingleTimeInput
                          label={timeLabel}
                          time={selectedTime[0]}
                          onTimeChange={(time) => setSelectedTime([time])}
                        />
                      </RowContainer>
                    )}
                    <DatePicker
                      allowDeselect
                      minDate={minDate}
                      maxDate={maxDate}
                      firstDayOfWeek={0}
                      {...datePickerProps}
                      type="default"
                      value={selectedDates[0]}
                      onChange={handleSingleDatePickerChange}
                      date={calendarDisplayDate}
                      onDateChange={setCalendarDisplayDate}
                    />
                  </>
                )}
                {isRangeType && (
                  <>
                    <RowContainer>
                      <SingleDateInput
                        label={startDateLabel}
                        selectedDate={selectedDates[0]}
                        onDateChange={handleStartDateChange}
                      />
                      <SingleDateInput
                        label={endDateLabel}
                        selectedDate={selectedDates[1]}
                        onDateChange={handleEndDateChange}
                      />
                    </RowContainer>
                    {type === 'date-time-range' && (
                      <RowContainer>
                        <SingleTimeInput
                          label={startTimeLabel}
                          time={selectedTime[0]}
                          onTimeChange={(time) =>
                            setSelectedTime((currentTime) => [
                              time,
                              currentTime[1],
                            ])
                          }
                        />
                        <SingleTimeInput
                          label={endTimeLabel}
                          time={selectedTime[1]}
                          onTimeChange={(time) =>
                            setSelectedTime((currentTime) => [
                              currentTime[0],
                              time,
                            ])
                          }
                        />
                      </RowContainer>
                    )}
                    <DatePicker
                      minDate={minDate}
                      maxDate={maxDate}
                      firstDayOfWeek={0}
                      {...dateRangePickerProps}
                      // those props can not be overridden at the moment,
                      // can combine them with the dateRangePickerProps when needed
                      ref={rangeCalendarRef}
                      type="range"
                      value={[
                        selectedDates[0] ?? null,
                        selectedDates[1] ?? null,
                      ]}
                      onChange={handleDateRangePickerChange}
                      date={calendarDisplayDate}
                      onDateChange={setCalendarDisplayDate}
                    />
                  </>
                )}
                <TimezoneSelector
                  {...timezoneSelectorProps}
                  value={selectedTimezone}
                  onChange={handleTimezoneChange}
                  comboboxProps={{ withinPortal: false }}
                />
                {customizeSection}
              </DateInputContainer>
              <QuickActionContainer>
                <QuickActionSelector
                  type={type}
                  onSelect={handleQuickActionClick}
                  {...quickActionProps}
                />
              </QuickActionContainer>
            </HorizontalContainer>
            <FooterContainer>
              <DisplayedValueContainer>
                {isValid && getValueText()}
              </DisplayedValueContainer>
              <Button
                kind="secondary"
                background="transparent"
                onClick={handleCancelClick}
              >
                {cancelButtonLabel}
              </Button>
              <Button disabled={!isValid} onClick={handleApplyClick}>
                {applyButtonLabel}
              </Button>
            </FooterContainer>
          </Container>
        </Popover.Dropdown>
      </Popover>
    )
  }
)

/** Get the display string for a selected date(range) and time(range) */
function getDefaultValueText(
  isValid: boolean,
  type: DateTimeType,
  selectedDates: [DateValue] | [DateValue, DateValue] | [],
  selectedTime:
    | [string | undefined]
    | [string | undefined, string | undefined]
    | []
) {
  if (!isValid) {
    return ''
  }

  return getDisplayText(type, selectedDates, selectedTime)
}

/**
 * Apply default start time value when no start time is set,
 * same for end time.
 */
function setDefaultStartOrEndTime(
  timePosition: 0 | 1,
  setSelectedTime: Dispatch<
    SetStateAction<
      [] | [string | undefined] | [string | undefined, string | undefined]
    >
  >
) {
  setSelectedTime((times) => [
    timePosition === 0 && !times[0] ? '00:00' : times[0],
    timePosition === 1 && !times[1] ? '23:59' : times[1],
  ])
}
