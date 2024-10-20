import _ from 'lodash'
import { DateTime } from 'luxon'

const formats = {
  date: DateTime.DATE_MED, // "4 Oct 2019"
  dateTime: {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    hour12: false,
  },
  dateTimeLong: {
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    month: 'short',
    year: 'numeric',
    hour12: false,
  },
  dateTimeShort: {
    day: '2-digit',
    month: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    hour12: false,
  },
  dateTimeDetailed: {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  },
  dayShort: {
    weekday: 'short',
  },
  time: {
    hour: 'numeric',
    minute: '2-digit',
    hour12: false,
  },
  month: {
    month: 'long',
  },
  dateTimeDetailedWithShortMonth: {
    day: '2-digit',
    month: 'short',
    year: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    second: '2-digit',
    hour12: true,
  },
  dateShort: {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
  },
}

function parse(value) {
  const numberValue = value?.valueOf()
  if (_.isNumber(numberValue)) {
    return DateTime.fromMillis(numberValue)
  }

  return DateTime.fromISO(value)
}

function floorToZero(value) {
  const nextValue = value < 0 ? Math.ceil(value) : Math.floor(value)

  return Object.is(nextValue, -0) ? 0 : nextValue
}

function difference(value, options = {}) {
  const nextValue = !options.decimals ? floorToZero(value) : value

  return !Number.isNaN(nextValue) ? nextValue : null
}

function getDateTime(time, timezone, locale) {
  const dateTime = parse(time).setZone(timezone).setLocale(locale)

  return {
    addDays(days) {
      return getDateTime(dateTime.plus({ days }), timezone, locale)
    },

    addHours(hours) {
      return getDateTime(dateTime.plus({ hours }), timezone, locale)
    },

    addMilliseconds(milliseconds) {
      return getDateTime(dateTime.plus({ milliseconds }), timezone, locale)
    },

    addMinutes(minutes) {
      return getDateTime(dateTime.plus({ minutes }), timezone, locale)
    },

    addMonths(months) {
      return getDateTime(dateTime.plus({ months }), timezone, locale)
    },

    addSeconds(seconds) {
      return getDateTime(dateTime.plus({ seconds }), timezone, locale)
    },

    addWeeks(weeks) {
      return getDateTime(dateTime.plus({ weeks }), timezone, locale)
    },

    addYears(years) {
      return getDateTime(dateTime.plus({ years }), timezone, locale)
    },

    differenceInDays(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'days').days, options)
    },

    differenceInHours(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'hours').hours, options)
    },

    differenceInMilliseconds(time2, options = {}) {
      return difference(
        dateTime.diff(parse(time2), 'milliseconds').milliseconds,
        options
      )
    },

    differenceInMinutes(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'minutes').minutes, options)
    },

    differenceInMonths(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'months').months, options)
    },

    differenceInSeconds(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'seconds').seconds, options)
    },

    differenceInWeeks(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'weeks').weeks, options)
    },

    differenceInYears(time2, options = {}) {
      return difference(dateTime.diff(parse(time2), 'years').years, options)
    },

    endOfDay() {
      return getDateTime(dateTime.endOf('day'), timezone, locale)
    },

    endOfHour() {
      return getDateTime(dateTime.endOf('hour'), timezone, locale)
    },

    endOfMinute() {
      return getDateTime(dateTime.endOf('minute'), timezone, locale)
    },

    endOfMonth() {
      return getDateTime(dateTime.endOf('month'), timezone, locale)
    },

    endOfSecond() {
      return getDateTime(dateTime.endOf('second'), timezone, locale)
    },

    endOfWeek() {
      return getDateTime(dateTime.endOf('week'), timezone, locale)
    },

    endOfYear() {
      return getDateTime(dateTime.endOf('year'), timezone, locale)
    },

    format(format, formatTimezone = timezone, formatLocale = locale) {
      const dateTimeWithTimezone = dateTime
        .setZone(formatTimezone)
        .setLocale(formatLocale)

      const derivedFormat = _.camelCase(format)
      if (!derivedFormat || derivedFormat === 'utc') {
        return dateTimeWithTimezone.toUTC().toISO()
      }
      if (derivedFormat === 'timezone') {
        return dateTimeWithTimezone.toISO()
      }

      const formattedDate = dateTimeWithTimezone.toLocaleString(
        formats[derivedFormat] ?? format
      )
      if (formattedDate === 'Invalid DateTime') {
        return null
      }

      return formattedDate
    },

    get day() {
      return dateTime.day
    },

    get hour() {
      return dateTime.hour
    },

    get millisecond() {
      return dateTime.millisecond
    },

    get minute() {
      return dateTime.minute
    },

    get month() {
      return dateTime.month
    },

    get second() {
      return dateTime.second
    },

    get week() {
      return dateTime.weekNumber
    },

    get year() {
      return dateTime.year
    },

    isBetween(time2, time3) {
      return dateTime >= parse(time2) && dateTime <= parse(time3)
    },

    isEqual(time2) {
      return +dateTime === +parse(time2)
    },

    setTime(time2) {
      const iso = dateTime.toISO()
      if (iso == null) {
        return getDateTime()
      }

      const date = iso.split('T')[0]
      const offset = `+${iso.split('+')[1]}`

      return getDateTime(`${date}T${time2}${offset}`, timezone, locale)
    },

    startOfDay() {
      return getDateTime(dateTime.startOf('day'), timezone, locale)
    },

    startOfHour() {
      return getDateTime(dateTime.startOf('hour'), timezone, locale)
    },

    startOfMillisecond() {
      return getDateTime(dateTime.startOf('millisecond'), timezone, locale)
    },

    startOfMinute() {
      return getDateTime(dateTime.startOf('minute'), timezone, locale)
    },

    startOfMonth() {
      return getDateTime(dateTime.startOf('month'), timezone, locale)
    },

    startOfSecond() {
      return getDateTime(dateTime.startOf('second'), timezone, locale)
    },

    startOfWeek() {
      return getDateTime(dateTime.startOf('week'), timezone, locale)
    },

    startOfYear() {
      return getDateTime(dateTime.startOf('year'), timezone, locale)
    },

    valueOf() {
      const value = dateTime.valueOf()

      return Number.isNaN(value) ? undefined : value
    },
  }
}

getDateTime.now = (timezone, locale) =>
  getDateTime(DateTime.local(), timezone, locale)

export default getDateTime
