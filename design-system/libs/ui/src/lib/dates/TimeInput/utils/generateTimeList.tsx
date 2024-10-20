import dayjs from 'dayjs'

export interface GenerateTimeListProps {
  /**
   * inclusive
   * @default '00:00:00'
   * @example '09:00:00' '09:00'
   */
  startTime?: string
  /**
   * inclusive
   * @default '23:59:59'
   * @example '15:00:00' '15:00'
   */
  endTime?: string
  /** in milliseconds */
  interval: number
}

/**
 * Generate a list of times inclusive startTime and endTime by
 * the given interval.
 * @return a list of times in format 'HH:mm:ss'
 */
// support for 'HH:mm:ss:SSS' can be added when requested
export function generateTimeList({
  startTime = '00:00:00',
  endTime = '23:59:59',
  interval,
}: GenerateTimeListProps): string[] {
  const start = dayjs(`2001-01-01 ${startTime}`)
  const end = dayjs(`2001-01-01 ${endTime}`)

  const times = []
  let time = start
  while (time <= end) {
    times.push(time.format('HH:mm:ss'))
    time = time.add(interval, 'millisecond')
  }
  return times
}
