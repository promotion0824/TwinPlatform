import _ from 'lodash'
import { Duration } from 'luxon'

function parse(value) {
  if (_.isString(value)) {
    return Duration.fromISO(value)
  }
  return Duration.fromObject(value)
}

function getDuration(value) {
  const duration = parse(value)
  return {
    toUIString() {
      const extraS = (d) => (d === 1 ? '' : 's')
      const totalMonths = duration.as('months')
      if (totalMonths >= 1) {
        return `${totalMonths} Month${extraS(totalMonths)}`
      }
      const totalWeeks = duration.as('weeks')
      if (totalWeeks >= 1) {
        return `${totalWeeks} Week${extraS(totalWeeks)}`
      }
      const totalDays = duration.as('days')
      if (totalDays >= 1) {
        return `${totalDays} Day${extraS(totalDays)}`
      }
      const totalHours = duration.as('hours')
      if (totalHours >= 1) {
        return `${totalHours} Hour${extraS(totalHours)}`
      }
      const totalMinutes = duration.as('minutes')
      return `${totalMinutes} Minute${extraS(totalMinutes)}`
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
