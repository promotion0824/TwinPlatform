import { useRef, useState } from 'react'
import cx from 'classnames'
import { useDateTime } from 'hooks'
import Button from 'components/Button/Button'
import { useDropdown } from 'components/DropdownNew/Dropdown'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import Time from 'components/Time/Time'
import { useDatePicker } from './DatePickerContext'
import TimePicker from './TimePicker'
import styles from './Calendar.css'

export default function Calendar() {
  const dateTime = useDateTime()
  const datePicker = useDatePicker()
  const dropdown = useDropdown()

  const [value, setValue] = useState(datePicker.value)
  const [hoverDate, setHoverDate] = useState()

  const nowRef = useRef(dateTime.now())
  const [monthStartDate, setMonthStartDate] = useState(
    dateTime(value[0] ?? nowRef.current).startOfMonth()
  )
  const currentYearRef = useRef(nowRef.current.year)

  const derivedValue =
    value.length === 1 && hoverDate != null
      ? [dateTime(value[0]).format(), dateTime(hoverDate).format()].sort()
      : value

  const calendarStartDate = dateTime(monthStartDate).startOfWeek()
  const currentMonth = dateTime(monthStartDate).month
  const isCurrentYear = dateTime(monthStartDate).year === currentYearRef.current

  const dates = Array.from(Array(42)).map((n, i) =>
    dateTime(calendarStartDate).addDays(i)
  )
  const weekDates = dates.slice(0, 7)

  function handlePrevMonthClick() {
    setMonthStartDate((prevMonthStartDate) =>
      dateTime(prevMonthStartDate).addMonths(-1)
    )
  }

  function handleNextMonthClick() {
    setMonthStartDate((prevMonthStartDate) =>
      dateTime(prevMonthStartDate).addMonths(1)
    )
  }

  function handleClick(date) {
    if (datePicker.type === 'date-range') {
      if (value.length === 1) {
        let nextDateRange = [dateTime(value[0]).format(), date.format()].sort()
        nextDateRange = [
          nextDateRange[0],
          dateTime(nextDateRange[1]).endOfDay().format(),
        ]

        datePicker.select(nextDateRange, true)
        dropdown.close()
      } else {
        setValue([date.format()])
      }
      return
    }

    if (datePicker.type === 'date-time-range') {
      if (value.length === 1) {
        let nextDateRange = [dateTime(value[0]).format(), date.format()].sort()
        nextDateRange = [
          nextDateRange[0],
          dateTime(nextDateRange[1]).endOfDay().format(),
        ]

        setValue(nextDateRange)
        datePicker.select(nextDateRange, true)
      } else {
        setValue([date.format()])
      }
      return
    }

    if (datePicker.type === 'date-time') {
      setValue([date.format()])
      datePicker.select(date.format())
      return
    }

    datePicker.select(date.format())
    dropdown.close()
  }

  function handleClearClick() {
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-time-range'
    ) {
      datePicker.select([])
    } else {
      datePicker.select(null)
    }

    dropdown.close()
  }

  function handleMouseEnter(date) {
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-time-range'
    ) {
      setHoverDate(date)
    }
  }

  function handleMouseLeave() {
    if (
      datePicker.type === 'date-range' ||
      datePicker.type === 'date-time-range'
    ) {
      setHoverDate()
    }
  }

  function handleFromTimeChange(time) {
    if (datePicker.type === 'date-time-range') {
      const nextRange = [time, value[1]]

      setValue(nextRange)
      datePicker.select(nextRange)
    } else {
      setValue([time])
      datePicker.select(time)
    }
  }

  function handleToTimeChange(time) {
    const nextRange = [value[0], time]

    setValue(nextRange)
    datePicker.select(nextRange)
  }

  return (
    <Spacing horizontal>
      {datePicker.helper && (
        <Spacing fill="header" className={styles.helper}>
          <Spacing className={styles.helperContent}>
            {datePicker.helper}
          </Spacing>
          <div className={styles.footer} />
        </Spacing>
      )}
      <Spacing>
        <Spacing horizontal fill="content hidden">
          <Button
            icon="chevron"
            disabled={
              dateTime(
                dateTime(monthStartDate).addDays(-1).format()
              ).differenceInDays(datePicker.min) < 0
            }
            className={styles.prev}
            onClick={handlePrevMonthClick}
          />
          <Spacing horizontal align="center middle" size="small">
            <Text type="message" size="large">
              <Time
                key={monthStartDate}
                value={monthStartDate}
                format="month"
              />
            </Text>
            {!isCurrentYear && (
              <Text type="message" size="large">
                <Time value={monthStartDate} format="year" />
              </Text>
            )}
          </Spacing>
          <Button
            icon="chevron"
            disabled={
              dateTime(
                dateTime(monthStartDate).addMonths(1).addDays(-1).format()
              ).differenceInDays(datePicker.max) > 0
            }
            className={styles.next}
            onClick={handleNextMonthClick}
          />
        </Spacing>
        <div className={styles.days}>
          {weekDates.map((date) => (
            <Spacing key={date} align="center middle">
              <Text type="message" size="tiny" color="grey">
                <Time value={date} format="day-short" />
              </Text>
            </Spacing>
          ))}
        </div>
        <div className={styles.dates}>
          {dates.map((date) => {
            const from = +dateTime(derivedValue[0]).startOfDay() === +date
            const to = +dateTime(derivedValue[1]).startOfDay() === +date
            const isBetween =
              date >= dateTime(derivedValue[0]).startOfDay() &&
              date <= dateTime(derivedValue[1]).startOfDay()
            const disabled =
              dateTime(date).differenceInDays(datePicker.min) < 0 ||
              dateTime(date).differenceInDays(datePicker.max) > 0

            const cxDateClassName = cx(styles.date, {
              [styles.from]: from,
              [styles.to]: to,
              [styles.isBetween]: isBetween,
              [styles.isInMonth]: currentMonth === date.month,
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
                <Spacing align="center middle" className={styles.content}>
                  {date.day}
                </Spacing>
              </Button>
            )
          })}
        </div>
        {(datePicker.type === 'date-time' ||
          datePicker.type === 'date-time-range') && (
          <>
            <hr />
            <Spacing
              horizontal
              fill="equal"
              size="medium"
              padding="medium large"
            >
              <TimePicker
                label={datePicker.type === 'date-time' ? 'Time' : 'From'}
                max={value[1]}
                value={value[0]}
                onChange={handleFromTimeChange}
              />
              {datePicker.type === 'date-time-range' ? (
                <TimePicker
                  label="To"
                  min={value[0]}
                  value={value[1]}
                  onChange={handleToTimeChange}
                />
              ) : (
                <div />
              )}
            </Spacing>
          </>
        )}
        <Spacing
          align="right middle"
          padding="0 large"
          className={styles.footer}
        >
          <Button onClick={() => handleClearClick()}>
            <Text type="message" size="tiny">
              Clear
            </Text>
          </Button>
        </Spacing>
      </Spacing>
    </Spacing>
  )
}
