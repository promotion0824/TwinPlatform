import _ from 'lodash'
import { Duration } from 'luxon'
import { t } from 'i18next'

function parse(value) {
  if (_.isString(value)) {
    return Duration.fromISO(value)
  }
  return Duration.fromObject(value)
}

function getDuration(value, locale) {
  const duration = parse(value)
  return {
    toUIString() {
      const totalMonths = duration.as('months')
      if (totalMonths >= 1) {
        return totalMonths === 1
          ? t('plainText.oneMonthCap')
          : t('interpolation.numberOfMonths', { lng: locale, num: totalMonths })
      }
      const totalWeeks = duration.as('weeks')
      if (totalWeeks >= 1) {
        return totalWeeks === 1
          ? t('plainText.oneWeekCap')
          : t('interpolation.numberOfWeeks', { lng: locale, num: totalWeeks })
      }
      const totalDays = duration.as('days')
      if (totalDays >= 1) {
        return totalDays === 1
          ? t('plainText.oneDay')
          : t('interpolation.numberOfDays', { lng: locale, num: totalDays })
      }
      const totalHours = duration.as('hours')
      if (totalHours >= 1) {
        return totalHours === 1
          ? t('plainText.oneHour')
          : t('interpolation.numberOfHours', { lng: locale, num: totalHours })
      }
      const totalMinutes = duration.as('minutes')
      return totalMinutes === 1
        ? t('plainText.oneMinute')
        : t('interpolation.numberOfMinutes', { lng: locale, num: totalMinutes })
    },
    toDotnetString() {
      return duration.toFormat('dd.hh:mm:ss')
    },
    toISOString() {
      return duration.toISO()
    },
    milliseconds() {
      return duration.as('milliseconds')
    },
    isGreaterThan(input) {
      const inputDuration = Duration.isDuration(input) ? input : parse(input)
      return duration.valueOf() > inputDuration.valueOf()
    },
    isLessThan(input) {
      const inputDuration = Duration.isDuration(input) ? input : parse(input)
      return duration.valueOf() < inputDuration.valueOf()
    },
  }
}

export default getDuration
