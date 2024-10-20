import { DateTimeType } from '../types'

export const getDefaultPlaceholderByType = (type: DateTimeType) => {
  if (type === 'date') {
    return 'Pick date'
  }
  if (type === 'date-time') {
    return 'Pick date and time'
  }
  if (type === 'date-range') {
    return 'Pick date range'
  }
  if (type === 'date-time-range') {
    return 'Pick date and time range'
  }
  throw new Error(`Unknown DateTimeType: ${type}`)
}
