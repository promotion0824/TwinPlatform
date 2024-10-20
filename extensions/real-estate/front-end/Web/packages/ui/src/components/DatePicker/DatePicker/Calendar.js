/* eslint-disable complexity */
import { useRef, useState } from 'react'
import { styled } from 'twin.macro'
import cx from 'classnames'
import { useDateTime } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { DateTime } from 'luxon'
import Button from 'components/Button/Button'
import { useDropdown } from 'components/Dropdown/Dropdown'
import Flex from 'components/Flex/Flex'
import Text from 'components/Text/Text'
import Time from 'components/Time/Time'
import { useDatePicker } from './DatePickerContext'
import BusinessDayRange from './BusinessDayRange'
import TimePicker from './TimePicker'
import styles from './Calendar.css'
import QuickRangeOptions from './QuickRangeOptions'

/**
 * Get the DateTime from value for Time from input.
 * If the date-picker is of type 'date-time-range', if both from and to values are not
 * selected, then we return undefined to disable the Time from input.
 */
const getTimeFrom = (type, value) => {
  const [dateTimeFrom, dateTimeTo] = value
  if (type === 'date-time-range') {
    if (!dateTimeFrom || !dateTimeTo) {
      return undefined
    }
  }

  return dateTimeFrom
}

const getISODate = (dateTime) => dateTime.format('timezone').split('T')[0]

export default function Calendar({ featureFlags, hideBusinessHourRange }) {
  const dateTime = useDateTime()
  const datePicker = useDatePicker()

  const dropdown = useDropdown()
  const { t } = useTranslation()

  const [selectingValue, setSelectingValue] = useState(null)
  const value = selectingValue || datePicker.value // [DateTime, DateTime?]

  const [hoverDate, setHoverDate] = useState()

  const nowRef = useRef(dateTime.now(datePicker.timezone))
  const [monthStartISODate, setMonthStartISODate] = useState(
    getISODate(
      dateTime(value[0] ?? nowRef.current, datePicker.timezone).startOfMonth()
    )
  )

  const monthStartDate = dateTime(
    DateTime.fromISO(monthStartISODate, { zone: datePicker.timezone }),
    datePicker.timezone
  )

  const derivedValue =
    value.length === 1 && hoverDate != null
      ? [
          dateTime(value[0], datePicker.timezone).format(),
          dateTime(hoverDate, datePicker.timezone).format(),
        ].sort()
      : value

  const calendarStartDate = dateTime(
    monthStartDate,
    datePicker.timezone
  ).startOfWeek()

  const currentMonth = dateTime(monthStartDate, datePicker.timezone).month
  const isCurrentYear =
    dateTime(monthStartDate, datePicker.timezone).year === nowRef.current.year

  const dates = Array.from(Array(42)).map((n, i) =>
    dateTime(calendarStartDate, datePicker.timezone).addDays(i)
  )
  const weekDates = dates.slice(0, 7)
  const today = dateTime(nowRef.current, datePicker.timezone).startOfDay()

  function handlePrevMonthClick() {
    setMonthStartISODate(getISODate(monthStartDate.addMonths(-1)))
  }

  function handleNextMonthClick() {
    setMonthStartISODate(getISODate(monthStartDate.addMonths(1)))
  }

  function handlePrevYearClick() {
    setMonthStartISODate(getISODate(monthStartDate.addYears(-1)))
  }

  function handleNextYearClick() {
    setMonthStartISODate(getISODate(monthStartDate.addYears(1)))
  }

  function handleClick(date) {
    if (
      ['date-range', 'date-time-range', 'date-business-range'].includes(
        datePicker.type
      )
    ) {
      // reset quick option when anything other than quick option button is clicked
      datePicker.selectQuickRange?.(null)

      if (selectingValue) {
        let nextDateRange = [
          dateTime(value[0], datePicker.timezone).format(),
          date.format(),
        ].sort()

        nextDateRange = [
          nextDateRange[0],
          dateTime(nextDateRange[1], datePicker.timezone).endOfDay().format(),
        ]

        datePicker.select(nextDateRange, true)
        setSelectingValue(null)
      } else {
        setSelectingValue([date.format()])
      }
      return
    }

    if (datePicker.type === 'date-time') {
      datePicker.select(date.format())
      return
    }

    datePicker.select(date.format())
    dropdown.close()
  }

  // There's a side effect that occurs after this function is executed.
  // Depending on the function that handles changing the value for the datepicker.
  // if the value is [], it will reset the value to the default value.
  function handleClearClick() {
    datePicker.selectQuickRange?.(null)
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-business-range' ||
      datePicker.type === 'date-time-range'
    ) {
      datePicker.select([])
    } else {
      datePicker.select(null)
    }

    if (datePicker.type === 'date-business-range') {
      datePicker.onResetClick()
    }

    dropdown.close()
  }

  function handleMouseEnter(date) {
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-business-range' ||
      datePicker.type === 'date-time-range'
    ) {
      setHoverDate(date)
    }
  }

  function handleMouseLeave() {
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-business-range' ||
      datePicker.type === 'date-time-range'
    ) {
      setHoverDate()
    }
  }

  function handleFromTimeChange(time) {
    if (datePicker.type === 'date-time-range') {
      const nextRange = [time, value[1]]

      datePicker.select(nextRange)
    } else {
      datePicker.select(time)
    }
  }

  function handleToTimeChange(time) {
    const nextRange = [value[0], time]

    datePicker.select(nextRange)
  }

  function handleQuickRangeChange(nextQuickRange, nextDateRange) {
    // Update current month view.
    setMonthStartISODate(
      getISODate(dateTime(nextDateRange[0], datePicker.timezone).startOfMonth())
    )

    datePicker.selectQuickRange(nextQuickRange)
    datePicker.select(nextDateRange, nextQuickRange)
  }

  function handleDayRangeChange(dayRange) {
    datePicker.onDayRangeChange(dayRange)
  }

  function handleBusinessHourChange(hourChange) {
    datePicker.onBusinessHourRangeChange(hourChange)
  }

  return (
    <Flex horizontal>
      {datePicker.helper && (
        <Flex fill="header" className={styles.helper}>
          <Flex className={styles.helperContent}>{datePicker.helper}</Flex>
          <div className={styles.footer} />
        </Flex>
      )}
      <FlexDropdownHeader>
        <FlexWithMargin horizontal fill="content hidden">
          <RotatedButton
            aria-label={t('plainText.prevYear')}
            iconSize="large"
            icon="chevronBack"
            onClick={handlePrevYearClick}
          />
          <FlexButton
            aria-label={t('plainText.prevMonth')}
            flex={0}
            icon="chevron"
            disabled={
              dateTime(
                dateTime(monthStartDate, datePicker.timezone)
                  .addDays(-1)
                  .format(),
                datePicker.timezone
              ).differenceInDays(datePicker.min) < 0
            }
            className={styles.prev}
            onClick={handlePrevMonthClick}
          />
          <CenterContentFlexButton flex={1} align="center middle" size="small">
            <Text type="message" size="medium" weight="medium">
              <Time
                key={monthStartDate}
                value={monthStartDate}
                timezone={datePicker.timezone}
                format="month"
              />
            </Text>
            {!isCurrentYear && (
              <Text type="message" size="medium" weight="medium">
                <Time
                  value={monthStartDate}
                  timezone={datePicker.timezone}
                  format="year"
                />
              </Text>
            )}
          </CenterContentFlexButton>
          <Button
            aria-label={t('plainText.nextMonth')}
            icon="chevron"
            disabled={
              dateTime(
                dateTime(monthStartDate, datePicker.timezone)
                  .addMonths(1)
                  .addDays(-1)
                  .format(),
                datePicker.timezone
              ).differenceInDays(datePicker.max) > 0
            }
            className={styles.next}
            onClick={handleNextMonthClick}
          />
          <RotatedButton
            aria-label={t('plainText.nextYear')}
            degree="180deg"
            icon="chevronBack"
            iconSize="large"
            onClick={handleNextYearClick}
          />
        </FlexWithMargin>
        <div className={styles.days}>
          {weekDates.map((date) => (
            <Flex key={date} align="center middle">
              <Text type="message" size="tiny" color="grey" weight="extraBold">
                <Time
                  value={date}
                  timezone={datePicker.timezone}
                  format="day-short"
                />
              </Text>
            </Flex>
          ))}
        </div>
        <div data-testid="dates" className={styles.dates}>
          {dates.map((date) => {
            const from =
              +dateTime(derivedValue[0], datePicker.timezone).startOfDay() ===
              +date
            const to =
              +dateTime(derivedValue[1], datePicker.timezone).startOfDay() ===
              +date
            const isBetween =
              date >=
                dateTime(derivedValue[0], datePicker.timezone).startOfDay() &&
              date <=
                dateTime(derivedValue[1], datePicker.timezone).startOfDay()

            const dt = dateTime(date, datePicker.timezone)
            const disabled =
              dt.differenceInDays(datePicker.min) < 0 ||
              dt.differenceInDays(datePicker.max) > 0 ||
              // If we have a max date range, and we have selected one end of
              // the range (value.length === 1), then disable dates outside of
              // the range.
              (datePicker.maxDays != null &&
                value.length === 1 &&
                Math.abs(dt.differenceInDays(value[0])) > datePicker.maxDays)

            const cxDateClassName = cx(styles.date, {
              [styles.from]: from,
              [styles.to]: to,
              [styles.isBetween]: isBetween,
              [styles.isInMonth]: currentMonth === date.month,
              [styles.today]: dt.differenceInDays(today) === 0,
            })

            return (
              <Button
                key={date}
                className={cxDateClassName}
                selected={from}
                disabled={disabled}
                onClick={() => handleClick(date)}
                onMouseEnter={() => handleMouseEnter(date)}
                onMouseLeave={() => handleMouseLeave()}
              >
                <Flex align="center middle" className={styles.content}>
                  {date.day}
                </Flex>
              </Button>
            )
          })}
        </div>
        {['date-range', 'date-business-range', 'date-time-range'].includes(
          datePicker.type
        ) &&
          datePicker.quickRangeOptions?.length > 0 && (
            <StyledQuickOptions
              timezone={datePicker.timezone}
              options={datePicker.quickRangeOptions}
              onSelect={handleQuickRangeChange}
              selected={datePicker.selectedQuickRange}
              data-segment="Calendar Quick Time Option"
            />
          )}
        {featureFlags.hasFeatureToggle('businessHourRange') &&
          ['date-business-range'].includes(datePicker.type) && (
            <BusinessDayRange
              hideBusinessHourRange={hideBusinessHourRange}
              onDayRangeChange={handleDayRangeChange}
              selectedDayRange={datePicker.selectedDayRange}
              onBusinessHourRangeChange={handleBusinessHourChange}
              selectedBusinessHourRange={datePicker.selectedBusinessHourRange}
            />
          )}
        {(datePicker.type === 'date-time' ||
          datePicker.type === 'date-time-range') && (
          <>
            <Flex horizontal fill="equal" size="medium" padding="medium medium">
              <TimePicker
                label={
                  datePicker.type === 'date-time'
                    ? t('labels.time')
                    : t('labels.from')
                }
                zIndex={datePicker.zIndex}
                timezone={datePicker.timezone}
                max={value[1]}
                value={getTimeFrom(datePicker.type, value)}
                onChange={handleFromTimeChange}
              />
              {datePicker.type === 'date-time-range' ? (
                <TimePicker
                  label={t('labels.to')}
                  timezone={datePicker.timezone}
                  zIndex={datePicker.zIndex}
                  min={value[0]}
                  value={value[1]}
                  onChange={handleToTimeChange}
                />
              ) : (
                <div />
              )}
            </Flex>
            {datePicker.timezoneSelector != null && (
              <Flex padding="medium medium">{datePicker.timezoneSelector}</Flex>
            )}
          </>
        )}
        <Flex align="right middle" padding="0 large" className={styles.footer}>
          <Button onClick={handleClearClick}>
            <Text type="message" size="tiny">
              {t('plainText.reset')}
            </Text>
          </Button>
        </Flex>
      </FlexDropdownHeader>
    </Flex>
  )
}

const RotatedButton = styled(Button)(({ degree }) => ({
  transform: `rotate(${degree})`,
  '> svg': {
    color: 'var(--text)',
    stroke: 'none !important',
  },
  '> svg:hover': {
    color: '#D9D9D9',
  },
}))

const FlexButton = styled(Button)(({ flex }) => ({
  flex: `${flex || 'none'} !important`,
}))

const CenterContentFlexButton = styled(FlexButton)({
  display: 'flex',
  justifyContent: 'center',
  flexFlow: 'row',
})

const FlexDropdownHeader = styled(Flex)({
  width: '378px',
})

const FlexWithMargin = styled(Flex)({
  margin: '0px 8px',
})

const StyledQuickOptions = styled(QuickRangeOptions)({
  margin: '4px 9px',

  '& > [data-is-selected]': {
    backgroundColor: 'var(--purple-pressed)',
  },
})
