import {
  generateTimeList,
  type GenerateTimeListProps,
} from './generateTimeList'
import { timesToListItems, TimesToListItemsProps } from './timesToListItems'

export const generateTimeItems = ({
  startTime,
  endTime,
  interval,
  format,
}: GenerateTimeListProps & Pick<TimesToListItemsProps, 'format'>) => {
  const times = generateTimeList({
    startTime,
    endTime,
    interval,
  })
  return timesToListItems({
    times,
    format,
  })
}
