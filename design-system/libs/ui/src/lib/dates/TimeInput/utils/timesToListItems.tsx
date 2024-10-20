import dayjs from 'dayjs'

/**
 * Any format that is valid for dayjs with a combination of
 * 'H' 'HH' 'h' 'hh'
 * 'm' 'mm'
 * 's' 'ss'
 * 'a' 'A'
 */
export type TimeFormat = string

export interface TimesToListItemsProps {
  times: string[]
  /* @default 'hh:mm a' */
  format?: TimeFormat
}

export function timesToListItems({
  times,
  format = 'hh:mm a',
}: TimesToListItemsProps) {
  return times.map((time) => ({
    value: dayjs(`2001-01-01 ${time}`).format(
      format.includes('s') ? 'HH:mm:ss' : 'HH:mm'
    ),
    label: dayjs(`2001-01-01 ${time}`).format(format),
  }))
}
