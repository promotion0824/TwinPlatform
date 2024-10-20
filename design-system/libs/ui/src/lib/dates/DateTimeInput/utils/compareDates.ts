import dayjs from 'dayjs'

/**
 * compare date1 and date2,
 * @return 1 if date1 > date2
 * @return -1 if date1 < date2
 * @return 0 if date1 === date2
 */
export const compareDates = (date1: Date | string, date2: Date | string) => {
  const dayjsDate1 = dayjs(date1)
  const dayjsDate2 = dayjs(date2)

  if (dayjsDate1.isBefore(dayjsDate2)) {
    return -1
  }

  if (dayjsDate1.isAfter(dayjsDate2)) {
    return 1
  }

  return 0
}
