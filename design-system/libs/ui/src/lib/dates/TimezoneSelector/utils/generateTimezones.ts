import dayjs from 'dayjs'
import timezone from 'dayjs/plugin/timezone'
import utc from 'dayjs/plugin/utc'
import _ from 'lodash'

dayjs.extend(utc)
dayjs.extend(timezone)

export const generateTimezones = (): { label: string; value: string }[] =>
  generateOptions(Intl.supportedValuesOf('timeZone'))

export const generateOptions = (timezones: string[]) =>
  _.chain(timezones)
    .map((timezone) => ({
      label: `(${dayjs().tz(timezone).offsetName()}) ${timezone}`,
      value: timezone,
    }))
    // sort by offsetName alphabetically
    .sortBy('label')
    .orderBy(({ value, label }) => {
      if (label.includes('GMT')) {
        // sort GMT timezones by offset value
        return dayjs().tz(value).utcOffset()
      }

      return 'label'
    })
    .value()
