import { DateTime } from 'luxon'
import { t } from 'i18next'

const formats = {
  date: DateTime.DATE_MED, // "4 Oct 2019"
  dateTime: {
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
  dayNarrow: {
    weekday: 'narrow',
  },
  dayShort: {
    weekday: 'short',
  },
  day: {
    weekday: 'long',
  },
  time: {
    hour: 'numeric',
    minute: '2-digit',
    hour12: false,
  },
  month: {
    month: 'long',
  },
  monthAndYear: {
    month: 'short',
    year: 'numeric',
  },
  year: {
    year: 'numeric',
  },
}

function formatDateTime(dateTime, formatValue) {
  let formattedDate = dateTime.toLocaleString(
    formats[formatValue] ?? formatValue
  )
  if (
    formatValue === 'time' ||
    formatValue === 'dateTime' ||
    formatValue === 'dateTimeLong'
  ) {
    formattedDate = formattedDate.replace(/24:/, '00:')
  }
  if (formattedDate === 'Invalid DateTime') {
    return null
  }

  return formattedDate
}

export function format(dateTime, formatValue) {
  const formattedDate = formatDateTime(dateTime, formatValue)

  if (formattedDate != null) {
    if (formatValue === 'at') {
      return t('interpolation.atDateTime', {
        dateTime: formatDateTime(dateTime, 'dateTime'),
      })
    }

    if (formatValue === 'by') {
      return t('interpolation.byDateTime', {
        dateTime: formatDateTime(dateTime, 'dateTime'),
      })
    }
  }

  return formattedDate
}

export function formatAgo(dateTime, now, locale = 'en') {
  const years = Math.abs(now.differenceInYears(dateTime))
  const months = Math.abs(now.differenceInMonths(dateTime))
  const days = Math.abs(now.differenceInDays(dateTime))
  const hours = Math.abs(now.differenceInHours(dateTime))
  const minutes = Math.abs(now.differenceInMinutes(dateTime))
  const ms = now.differenceInMilliseconds(dateTime)

  if (ms <= 0) {
    return '-'
  }

  let str
  if (years > 0)
    str = t('interpolation.numberOfYearsAgoShort', {
      defaultValue: `${years}y ago`,
      lng: locale,
      num: years,
    })
  else if (months > 0)
    str = t('interpolation.numberOfMonthsAgoShort', {
      defaultValue: `${months}mo ago`,
      lng: locale,
      num: months,
    })
  else if (days > 0)
    str = t('interpolation.numberOfDaysAgoShort', {
      defaultValue: `${days}d ago`,
      lng: locale,
      num: days,
    })
  else if (hours > 0)
    str = t('interpolation.numberOfHoursAgoShort', {
      defaultValue: `${hours}h ago`,
      lng: locale,
      num: hours,
    })
  else
    str = t('interpolation.numberOfMinutesAgoShort', {
      defaultValue: `${minutes}m ago`,
      lng: locale,
      num: minutes,
    })

  return str
}

export function formatAgoDetail(dateTime, now) {
  let time = now
  const years = Math.max(now.differenceInYears(dateTime), 0)
  time = time.addYears(-years)
  const months = Math.max(time.differenceInMonths(dateTime), 0)
  time = time.addMonths(-months)
  const days = Math.max(time.differenceInDays(dateTime), 0)
  time = time.addDays(-days)
  const hours = Math.max(time.differenceInHours(dateTime), 0)
  time = time.addHours(-hours)
  const minutes = Math.max(time.differenceInMinutes(dateTime), 0)
  const ms = now.differenceInMilliseconds(dateTime)

  if (ms <= 0) {
    return '-'
  }

  let str = ''
  if (years > 0) str += `${years}y `
  if (months > 0) str += `${months}mo `
  if (days > 0) str += `${days}d `
  if (hours > 0) str += `${hours}h `
  str += `${minutes}m`

  return `${str.trim()} ago`
}

export function formatIn(dateTime, now) {
  const years = Math.abs(now.differenceInYears(dateTime))
  const months = Math.abs(now.differenceInMonths(dateTime))
  const days = Math.abs(now.differenceInDays(dateTime))
  const hours = Math.abs(now.differenceInHours(dateTime))
  const minutes = Math.abs(now.differenceInMinutes(dateTime))
  const ms = now.differenceInMilliseconds(dateTime)

  if (ms >= 0) {
    return '-'
  }

  let str
  if (years > 0) str = t('interpolation.inNumberOfYearsShort', { num: years })
  else if (months > 0)
    str = t('interpolation.inNumberOfMonthsShort', { num: months })
  else if (days > 0) str = t('interpolation.inNumberOfDaysShort', { num: days })
  else if (hours > 0)
    str = t('interpolation.inNumberOfHoursShort', { num: hours })
  else str = t('interpolation.inNumberOfMinuteShort', { num: minutes })

  return str
}
