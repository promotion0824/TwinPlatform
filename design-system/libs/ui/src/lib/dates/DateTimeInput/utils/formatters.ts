import dayjs from 'dayjs'

export const dateFormat = 'DD MMM YYYY'
export const timeFormat = 'hh:mm A'

const stringifyDate = (date: Date) => dayjs(date).format(dateFormat)

const stringifyTime = (time: string) =>
  dayjs(`2001-01-01 ${time}`).format(timeFormat)

export const stringifyDateTime = (date: Date, time?: string) =>
  time ? `${stringifyDate(date)}, ${stringifyTime(time)}` : stringifyDate(date)

/** This will return the time format that TimeInput can accept */
export const getTimeFromDate = (date: Date) => dayjs(date).format('HH:mm')
